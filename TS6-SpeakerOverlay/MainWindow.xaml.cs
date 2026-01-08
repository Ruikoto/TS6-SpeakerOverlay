using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TS6_SpeakerOverlay.Helpers;
using TS6_SpeakerOverlay.ViewModels;

// [修复关键] 指定 KeyEventArgs 使用 WPF 的版本，解决 CS0104 错误
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace TS6_SpeakerOverlay
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _topmostTimer;
        private TrayIconHelper? _trayIcon; 

        public MainWindow()
        {
            // 如果这里 InitializeComponent 还报错，先不管，修好上面两个它自己就好了
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.MouseDown += MainWindow_MouseDown;
            this.Closing += MainWindow_Closing;
            this.KeyDown += MainWindow_KeyDown; 

            // 暴力置顶
            _topmostTimer = new DispatcherTimer();
            _topmostTimer.Interval = TimeSpan.FromSeconds(2); 
            _topmostTimer.Tick += (s, e) => WindowHelper.ForceTopMost(this);
            _topmostTimer.Start();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = 50;
            this.Top = 50;

            // 初始化托盘
            _trayIcon = new TrayIconHelper(
                this,
                GetIsLocked,
                Lock,
                Unlock,
                (trayIcon) => _trayIcon = trayIcon
            );
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
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

        // [修复] 这里现在的参数类型明确为 System.Windows.Input.KeyEventArgs
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