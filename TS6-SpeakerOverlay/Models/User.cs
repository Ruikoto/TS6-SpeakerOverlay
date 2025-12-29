using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace TS6_SpeakerOverlay.Models
{
    public partial class User : ObservableObject
    {
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private bool _isTalking;
        
        // 新增：TS6 内部的用户ID
        [ObservableProperty] private int _clientId;
        // 新增：用户所在的频道ID
        [ObservableProperty] private string _channelId = string.Empty;

        public DateTime LastTalkTime { get; set; } = DateTime.MinValue;
    }
}