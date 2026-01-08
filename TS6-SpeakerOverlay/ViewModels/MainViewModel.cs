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
        
        private readonly Ts6Service _tsService;
        private string _currentChannelId = ""; 

        [ObservableProperty] private bool _isOverlayLocked = false;

        public MainViewModel()
        {
            _tsService = new Ts6Service();
            
            _tsService.OnChannelListUpdated += (allUsers, myChannelId) => 
            {
                _currentChannelId = myChannelId;
                // [修复] 明确指定 System.Windows.Application
                System.Windows.Application.Current.Dispatcher.Invoke(() => 
                {
                    Users.Clear();
                    var roomUsers = allUsers.Where(u => u.ChannelId == _currentChannelId).OrderBy(u => u.Name);
                    foreach(var u in roomUsers) Users.Add(u);
                });
            };

            _tsService.OnTalkStatusChanged += (clientId, isTalking) =>
            {
                // [修复] 明确指定 System.Windows.Application
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var user = Users.FirstOrDefault(u => u.ClientId == clientId);
                    if (user != null) user.IsTalking = isTalking;
                });
            };

            _tsService.OnUserPropertiesChanged += (clientId, inMute, outMute, away) =>
            {
                // [修复] 明确指定 System.Windows.Application
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
                // [修复] 明确指定 System.Windows.Application
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
    }
}