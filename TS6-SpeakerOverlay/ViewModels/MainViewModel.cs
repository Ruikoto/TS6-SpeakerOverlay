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

        [ObservableProperty] private bool _isOverlayLocked;

        public MainViewModel()
        {
            _tsService = new Ts6Service();

            // 监听锁定状态变化
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(IsOverlayLocked))
                {
                    if (IsOverlayLocked)
                    {
                        ShowNotification("已锁定，点击托盘图标可解锁", "#5E5CE6", "🔒");
                    }
                    else
                    {
                        ShowNotification("已解锁", "#4FCD8E", "🔓");
                    }
                }
            };

            // 订阅事件：连接成功
            _tsService.OnConnected += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowNotification("正在连接到 TeamSpeak...", "#5E5CE6", "🔄");
                    Users.Clear();
                    Users.Add(new User { Name = "等待 TS6 响应..." });
                });
            };

            // 订阅事件：连接断开
            _tsService.OnDisconnected += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowNotification("连接断开，正在重连...", "#F04747", "⚠️");

                    _allUsers.Clear();
                    _currentChannelId = "";
                    _myClientId = 0;
                    Users.Clear();
                    Users.Add(new User { Name = "正在重连 TS6..." });
                });
            };

            // 订阅事件：TS6 开始连接服务器
            _tsService.OnServerConnecting += () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowNotification("正在连接服务器...", "#5E5CE6", "🔄");

                    // Clear user list, keep myClientId to identify ourselves in clientMoved events
                    _allUsers.Clear();
                    _currentChannelId = "";
                    Users.Clear();
                    Users.Add(new User { Name = "正在加载..." });
                });
            };

            // 订阅事件：初始化用户列表
            _tsService.OnChannelListUpdated += (allUsers, myChannelId, myClientId) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    // If we already have the same identity, this is a refresh to update usernames
                    if (_myClientId != 0 && _myClientId == myClientId && _currentChannelId == myChannelId)
                    {
                        // Update existing users' names from auth response
                        foreach (var authUser in allUsers)
                        {
                            var cachedUser = _allUsers.FirstOrDefault(u => u.ClientId == authUser.ClientId);
                            if (cachedUser != null)
                            {
                                cachedUser.Name = authUser.Name;
                                cachedUser.ChannelId = authUser.ChannelId;
                                cachedUser.IsTalking = authUser.IsTalking;
                            }
                            else
                            {
                                // Add missing users
                                _allUsers.Add(authUser);
                            }
                        }

                        RefreshUserList();
                    }
                    else
                    {
                        // Initial connection - replace entire list
                        _currentChannelId = myChannelId;
                        _myClientId = myClientId;
                        _allUsers = allUsers;

                        RefreshUserList();
                        Console.WriteLine(
                            $"[UI] 初始化列表，我的ID: {_myClientId}, 频道: {_currentChannelId}, 人数: {Users.Count}");

                        ShowNotification("已连接到 TeamSpeak", "#43B581", "✅");
                    }
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
                    var cachedUser = _allUsers.FirstOrDefault(u => u.ClientId == clientId);
                    if (cachedUser == null)
                    {
                        cachedUser = new User
                        {
                            ClientId = clientId,
                            Name = "加载中...",
                            ChannelId = newChannelId,
                            IsTalking = false
                        };
                        _allUsers.Add(cachedUser);
                    }
                    else
                    {
                        cachedUser.ChannelId = newChannelId;
                    }

                    // Auto-detect identity when reconnecting: first user to non-0 channel is likely me
                    if ((_currentChannelId == "0" || _currentChannelId == "" || _myClientId == 0)
                        && newChannelId != "0"
                        && _allUsers.Count <= 2)
                    {
                        _myClientId = clientId;
                        _currentChannelId = newChannelId;
                        RefreshUserList();
                        ShowNotification($"已重新连接，当前 {Users.Count} 人", "#43B581", "✅");
                        return;
                    }

                    // Handle my own channel changes
                    if (_myClientId != 0 && clientId == _myClientId)
                    {
                        _currentChannelId = newChannelId;
                        RefreshUserList();

                        if (newChannelId == "0")
                        {
                            ShowNotification("已断开服务器连接", "#F04747", "📤");
                        }
                        else
                        {
                            ShowNotification($"已切换频道，当前 {Users.Count} 人", "#5E5CE6", "🔄");
                        }
                    }
                    else
                    {
                        // Handle other users' channel changes
                        var wasInMyChannel = Users.Any(u => u.ClientId == clientId);
                        RefreshUserList();
                        var isNowInMyChannel = Users.Any(u => u.ClientId == clientId);

                        if (!wasInMyChannel && isNowInMyChannel)
                        {
                            ShowNotification($"{cachedUser.Name} 加入了频道", "#43B581", "📥");
                        }
                        else if (wasInMyChannel && !isNowInMyChannel)
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
                        if (cachedUser.ChannelId == _currentChannelId)
                        {
                            ShowNotification($"{cachedUser.Name} 离开了频道", "#F04747", "📤");
                        }

                        _allUsers.Remove(cachedUser);
                    }

                    RefreshUserList();
                });
            };

            // 订阅事件：用户属性更新
            _tsService.OnClientPropertiesUpdated += (clientId, nickname) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var cachedUser = _allUsers.FirstOrDefault(u => u.ClientId == clientId);
                    if (cachedUser != null)
                    {
                        cachedUser.Name = nickname;

                        var displayedUser = Users.FirstOrDefault(u => u.ClientId == clientId);
                        displayedUser?.Name = nickname;
                    }
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
            try
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

                Notifications.Remove(notification);
            }
            catch (Exception)
            {
                // suppress
            }
        }
    }
}