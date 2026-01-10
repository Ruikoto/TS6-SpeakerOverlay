using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TS6_SpeakerOverlay.Helpers;
using TS6_SpeakerOverlay.ViewModels;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using TS6_SpeakerOverlay.Views;

namespace TS6_SpeakerOverlay
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _topmostTimer;
        private TrayIconHelper? _trayIcon; 

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.MouseDown += MainWindow_MouseDown;
            this.Closing += MainWindow_Closing;
            this.KeyDown += MainWindow_KeyDown; 

            _topmostTimer = new DispatcherTimer();
            _topmostTimer.Interval = TimeSpan.FromSeconds(2); 
            _topmostTimer.Tick += (s, e) => WindowHelper.ForceTopMost(this);
            _topmostTimer.Start();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 初始化托盘
            _trayIcon = new TrayIconHelper(
                this,
                GetIsLocked,
                Lock,
                Unlock,
                OpenSettings,
                (trayIcon) => _trayIcon = trayIcon
            );
            
            // 注意：因为我们在 XAML 里双向绑定了 Left/Top 到 Config.WindowLeft/WindowTop
            // 所以这里不需要手动写代码赋值，WPF 会自动把 Config 里的值应用到 Window 上
        }
                private void OpenSettings()
        {
            if (DataContext is MainViewModel vm)
            {
                // 创建并显示设置窗口，传入当前的 ViewModel
                var settingsWindow = new SettingsWindow(vm);
                settingsWindow.Show();
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // 保存配置 (位置信息会自动同步到 Config 对象，只需调用 Save)
            if (DataContext is MainViewModel vm)
            {
                vm.SaveConfig();
            }

            if (_trayIcon == null) return;
            e.Cancel = true;
            this.Hide();
            _trayIcon.UpdateTrayIcon();
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !GetIsLocked())
            {
                this.DragMove();
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ToggleLock();
                e.Handled = true;
            }
        }

        private bool GetIsLocked()
        {
            return (DataContext is MainViewModel vm) && vm.IsOverlayLocked;
        }

        private void Lock()
        {
            if (DataContext is MainViewModel vm && !vm.IsOverlayLocked)
            {
                WindowHelper.EnableClickThrough(this);
                vm.IsOverlayLocked = true;
                _trayIcon?.UpdateTrayIcon();
            }
        }

        private void Unlock()
        {
            if (DataContext is MainViewModel vm && vm.IsOverlayLocked)
            {
                WindowHelper.DisableClickThrough(this);
                vm.IsOverlayLocked = false;
                _trayIcon?.UpdateTrayIcon();
            }
        }

        private void ToggleLock()
        {
            if (GetIsLocked()) Unlock(); else Lock();
        }

        protected override void OnClosed(EventArgs e)
        {
            _trayIcon?.Dispose();
            base.OnClosed(e);
        }
    }
}