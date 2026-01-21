using CommunityToolkit.Mvvm.ComponentModel;

namespace TS6_SpeakerOverlay.Models
{
    public partial class Notification : ObservableObject
    {
        [ObservableProperty] private string _message = string.Empty;
        [ObservableProperty] private string _color = "#FFFFFF";
        [ObservableProperty] private string _icon = "";
    }
}
