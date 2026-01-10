using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows; 
using TS6_SpeakerOverlay.Models;
using TS6_SpeakerOverlay.Helpers;
using TS6_SpeakerOverlay.Services;

namespace TS6_SpeakerOverlay.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public ObservableCollection<User> Users { get; } = new();
        public ObservableCollection<Notification> Notifications { get; } = new();
        
        // 新增：配置对象，供界面绑定
        public AppConfig Config { get; }

        private readonly Ts6Service _tsService;
        private string _currentChannelId = ""; 

        [ObservableProperty] private bool _isOverlayLocked = false;

        public MainViewModel()
        {
            // 1. 加载配置
            Config = ConfigService.Load();

            _tsService = new Ts6Service();
            
            _tsService.OnChannelListUpdated += (allUsers, myChannelId) => 
            {
                _currentChannelId = myChannelId;
                System.Windows.Application.Current.Dispatcher.Invoke(() => 
                {
                    Users.Clear();
                    var roomUsers = allUsers.Where(u => u.ChannelId == _currentChannelId).OrderBy(u => u.Name);
                    foreach(var u in roomUsers) Users.Add(u);
                });
            };

            _tsService.OnTalkStatusChanged += (clientId, isTalking) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var user = Users.FirstOrDefault(u => u.ClientId == clientId);
                    if (user != null) user.IsTalking = isTalking;
                });
            };

            _tsService.OnUserPropertiesChanged += (clientId, inMute, outMute, away) =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var user = Users.FirstOrDefault(u => u.ClientId == clientId);
                    if (user != null)
                    {
                        if (inMute.HasValue) user.IsInputMuted = inMute.Value;
                        if (outMute.HasValue) user.IsOutputMuted = outMute.Value;
                        if (away.HasValue) user.IsAway = away.Value;
                    }
                });
            };

            _tsService.OnClientMoved += (clientId, newCh, oldCh) => 
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => 
                {
                    if (newCh == _currentChannelId)
                    {
                        ShowNotification("有新成员进入频道", "#43B581", "📥");
                    }
                    else if (oldCh == _currentChannelId)
                    {
                        var user = Users.FirstOrDefault(u => u.ClientId == clientId);
                        if (user != null)
                        {
                            ShowNotification($"{user.Name} 离开了频道", "#F04747", "📤");
                            Users.Remove(user);
                        }
                    }
                });
            };

            Task.Run(async () => await _tsService.StartAsync());
            Users.Add(new User { Name = "Connecting..." });
        }

        private async void ShowNotification(string msg, string color, string icon)
        {
            // 2. 判断配置开关：如果用户关了通知，直接返回
            if (!Config.EnableNotifications) return;

            var note = new Notification { Message = msg, Color = color, Icon = icon };
            Notifications.Add(note);
            await Task.Delay(3000);
            if (Notifications.Contains(note)) Notifications.Remove(note);
        }

        [RelayCommand]
        private void ToggleLockState(Window window)
        {
            IsOverlayLocked = !IsOverlayLocked;
            if (IsOverlayLocked) WindowHelper.EnableClickThrough(window);
            else WindowHelper.DisableClickThrough(window);
        }
        
        // 新增：保存配置的方法（给 MainWindow 关闭时调用）
        public void SaveConfig()
        {
            ConfigService.Save(Config);
        }
    }
}