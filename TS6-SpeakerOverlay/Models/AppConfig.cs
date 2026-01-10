using CommunityToolkit.Mvvm.ComponentModel;

namespace TS6_SpeakerOverlay.Models
{
    public partial class AppConfig : ObservableObject
    {
        // --- 窗口位置 ---
        [ObservableProperty] private double _windowTop = 50;
        [ObservableProperty] private double _windowLeft = 50;

        // --- 外观设置 (Appearance) ---
        [ObservableProperty] private double _uiScale = 1.0; 
        [ObservableProperty] private double _backgroundOpacity = 0.6; // 调低默认值，更通透
        [ObservableProperty] private double _fontSize = 14.0;
        [ObservableProperty] private double _itemSpacing = 4.0;
        [ObservableProperty] private double _avatarSize = 20.0; // [新增] 头像大小

        // --- 行为设置 (Behavior) ---
        [ObservableProperty] private bool _enableNotifications = true;
        [ObservableProperty] private bool _showOnlyTalking = false; // [新增] 仅显示说话者
        [ObservableProperty] private bool _alignRight = false; // [新增] 靠右对齐(预留功能)

        // 重置默认的方法
        public void ResetDefaults()
        {
            UiScale = 1.0;
            BackgroundOpacity = 0.6;
            FontSize = 14.0;
            ItemSpacing = 4.0;
            AvatarSize = 20.0;
            EnableNotifications = true;
            ShowOnlyTalking = false;
        }
    }
}