using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace TS6_SpeakerOverlay.Models
{
    public partial class User : ObservableObject
    {
        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private bool _isTalking;
        
        // --- 新增状态属性 ---
        [ObservableProperty] private bool _isInputMuted;  // 闭麦
        [ObservableProperty] private bool _isOutputMuted; // 关声 (耳机被ban)
        [ObservableProperty] private bool _isAway;        // 离开/AFK
        // ------------------

        [ObservableProperty] private int _clientId;
        [ObservableProperty] private string _channelId = string.Empty;

        public DateTime LastTalkTime { get; set; } = DateTime.MinValue;
    }
}