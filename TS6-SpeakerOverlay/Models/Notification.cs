
using CommunityToolkit.Mvvm.ComponentModel;

namespace TS6_SpeakerOverlay.Models
{
    public partial class Notification : ObservableObject
    {
        public string Message { get; set; } = string.Empty;
        public string Color { get; set; } = "#FFFFFF"; // 气泡颜色 (绿/橙)
        public string Icon { get; set; } = ""; // 图标 (Unicode)
    }
}