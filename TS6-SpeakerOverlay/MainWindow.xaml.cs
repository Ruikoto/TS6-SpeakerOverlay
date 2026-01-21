using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using TS6_SpeakerOverlay.Helpers;
using TS6_SpeakerOverlay.ViewModels;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace TS6_SpeakerOverlay
{
    public partial class MainWindow : Window
    {
        private TrayIconHelper? _trayIcon;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            KeyDown += MainWindow_KeyDown;

            // 添加鼠标进入/离开事件来控制工具栏显示
            MouseEnter += MainWindow_MouseEnter;
            MouseLeave += MainWindow_MouseLeave;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Left = 100;
            Top = 100;

            _trayIcon = new TrayIconHelper(
                this,
                GetIsLocked,
                Lock,
                Unlock,
                (trayIcon) => _trayIcon = trayIcon
            );
        }

        private void MainWindow_MouseEnter(object sender, MouseEventArgs e)
        {
            // 鼠标进入窗口区域时，淡入显示工具栏
            if (DataContext is MainViewModel vm && !vm.IsOverlayLocked)
            {
                var storyboard = new Storyboard();
                var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200));
                Storyboard.SetTarget(fadeIn, ToolBar);
                Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity"));
                storyboard.Children.Add(fadeIn);
                storyboard.Begin();
            }
        }

        private void MainWindow_MouseLeave(object sender, MouseEventArgs e)
        {
            // 鼠标离开窗口区域时，淡出隐藏工具栏
            var storyboard = new Storyboard();
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            Storyboard.SetTarget(fadeOut, ToolBar);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOut);
            storyboard.Begin();
        }

        private void DragHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 只有在拖拽按钮上按下左键时才允许拖动
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
                e.Handled = true;
            }
        }

        private void LockButton_Click(object sender, MouseButtonEventArgs e)
        {
            Lock();
            e.Handled = true;
        }

        private void ExitButton_Click(object sender, MouseButtonEventArgs e)
        {
            _trayIcon?.Dispose();
            _trayIcon = null;
            Application.Current.Shutdown();
            e.Handled = true;
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+L 切换锁定/解锁
            if (e.Key == Key.L && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ToggleLock();
                e.Handled = true;
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // If tray icon is null, application is shutting down
            if (_trayIcon == null)
            {
                return;
            }

            // 阻止窗口关闭，改为隐藏
            e.Cancel = true;
            Hide();
            _trayIcon.UpdateTrayIcon();
        }

        private bool GetIsLocked()
        {
            if (DataContext is MainViewModel vm)
            {
                return vm.IsOverlayLocked;
            }

            return false;
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
            if (GetIsLocked())
                Unlock();
            else
                Lock();
        }

        protected override void OnClosed(EventArgs e)
        {
            _trayIcon?.Dispose();
            base.OnClosed(e);
        }
    }
}