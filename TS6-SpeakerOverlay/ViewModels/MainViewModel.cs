using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TS6_SpeakerOverlay.Models;
using TS6_SpeakerOverlay.Helpers;
using TS6_SpeakerOverlay.Services;

namespace TS6_SpeakerOverlay.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly Ts6Service _tsService;
        private string _currentChannelId = "";
        private int _myClientId;
        private List<User> _allUsers = [];

        // UI 绑定的用户列表
        public ObservableCollection<User> Users { get; } = [];

        // 通知列表
        public ObservableCollection<Notification> Notifications { get; } = [];

        [ObservableProperty]
        private bool _isOverlayLocked;

        public MainViewModel()
        {
            _tsService = new Ts6Service();

            // 监听锁定状态变化
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsOverlayLocked) && IsOverlayLocked)
                {
                    ShowNotification("已锁定，点击托盘图标可解锁", "#5E5CE6", "🔒");
                }
            };

            // 订阅事件：初始化用户列表
            _tsService.OnChannelListUpdated += (allUsers, myChannelId, myClientId) =>
            {
                _currentChannelId = myChannelId;
                _myClientId = myClientId;
                _allUsers = allUsers;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    RefreshUserList();
                    Console.WriteLine($"[UI] 初始化列表，我的ID: {_myClientId}, 频道: {_currentChannelId}, 人数: {Users.Count}");

                    // 显示连接成功通知
                    ShowNotification("已连接到 TeamSpeak", "#43B581", "✅");
                });
            };

            // 订阅事件：说话状态改变
            _tsService.OnTalkStatusChanged += (clientId, isTalking) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var user = Users.FirstOrDefault(u => u.ClientId == clientId);
                    if (user != null)
                    {
                        user.IsTalking = isTalking;
                    }
                });
            };

            // 订阅事件：用户切换频道
            _tsService.OnClientMoved += (clientId, newChannelId) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    // 更新缓存中的用户频道ID
                    var cachedUser = _allUsers.FirstOrDefault(u => u.ClientId == clientId);
                    if (cachedUser != null)
                    {
                        cachedUser.ChannelId = newChannelId;
                    }

                    // 如果是我自己切换了频道
                    if (clientId == _myClientId)
                    {
                        _currentChannelId = newChannelId;
                        RefreshUserList();
                        Console.WriteLine($"[UI] 我切换到频道: {newChannelId}, 人数: {Users.Count}");

                        // 显示频道切换通知
                        ShowNotification($"已切换频道，当前 {Users.Count} 人", "#5E5CE6", "🔄");
                    }
                    else
                    {
                        // 其他用户切换频道
                        var wasInMyChannel = Users.Any(u => u.ClientId == clientId);
                        RefreshUserList();
                        var isNowInMyChannel = Users.Any(u => u.ClientId == clientId);

                        // 用户进入我的频道
                        if (!wasInMyChannel && isNowInMyChannel && cachedUser != null)
                        {
                            ShowNotification($"{cachedUser.Name} 加入了频道", "#43B581", "📥");
                        }
                        // 用户离开我的频道
                        else if (wasInMyChannel && !isNowInMyChannel && cachedUser != null)
                        {
                            ShowNotification($"{cachedUser.Name} 离开了频道", "#F04747", "📤");
                        }
                    }
                });
            };

            // 订阅事件：新用户进入视野
            _tsService.OnClientEnterView += (newUser) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_allUsers.All(u => u.ClientId != newUser.ClientId))
                    {
                        _allUsers.Add(newUser);
                        Console.WriteLine($"[UI] 新用户 {newUser.Name} 进入视野 (频道: {newUser.ChannelId})");

                        // 如果新用户在我的频道，显示通知
                        if (newUser.ChannelId == _currentChannelId)
                        {
                            ShowNotification($"{newUser.Name} 加入了频道", "#43B581", "📥");
                        }
                    }

                    RefreshUserList();
                });
            };

            // 订阅事件：用户离开视野
            _tsService.OnClientLeftView += (clientId) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var cachedUser = _allUsers.FirstOrDefault(u => u.ClientId == clientId);
                    if (cachedUser != null)
                    {
                        // 如果用户在我的频道，显示离开通知
                        if (cachedUser.ChannelId == _currentChannelId)
                        {
                            ShowNotification($"{cachedUser.Name} 离开了频道", "#F04747", "📤");
                        }

                        _allUsers.Remove(cachedUser);
                        Console.WriteLine($"[UI] 用户 {cachedUser.Name} 离开视野");
                    }

                    RefreshUserList();
                });
            };

            // 启动连接
            Task.Run(async () => await _tsService.StartAsync());

            // 初始占位符
            Users.Add(new User { Name = "正在连接 TS6..." });
        }

        [RelayCommand]
        private void ToggleLockState(Window window)
        {
            IsOverlayLocked = !IsOverlayLocked;
            if (IsOverlayLocked)
                WindowHelper.EnableClickThrough(window);
            else
                WindowHelper.DisableClickThrough(window);
        }

        /// <summary>
        /// 刷新用户列表：根据当前频道ID从缓存中筛选用户
        /// </summary>
        private void RefreshUserList()
        {
            Users.Clear();
            var roomUsers = _allUsers.Where(u => u.ChannelId == _currentChannelId).ToList();
            foreach (var u in roomUsers)
            {
                Users.Add(u);
            }
        }

        /// <summary>
        /// 显示通知消息
        /// </summary>
        private async void ShowNotification(string message, string color, string icon)
        {
            var notification = new Notification
            {
                Message = message,
                Color = color,
                Icon = icon
            };

            Notifications.Add(notification);

            // 3秒后自动移除通知
            await Task.Delay(3000);

            if (Notifications.Contains(notification))
            {
                Notifications.Remove(notification);
            }
        }
    }
}