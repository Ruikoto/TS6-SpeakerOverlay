using System.Windows;
using System.Windows.Input;
// 引用修正后的命名空间 (注意是下划线)
using TS6_SpeakerOverlay.Helpers;
using TS6_SpeakerOverlay.ViewModels;

namespace TS6_SpeakerOverlay
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // 如果这里报错，请先无视，按照第三步操作“清理解决方案”即可修复
            InitializeComponent();

            // 监听窗口加载
            this.Loaded += MainWindow_Loaded;
            
            // 监听鼠标按下
            this.MouseDown += MainWindow_MouseDown;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. 初始化位置 (例如左上角，或者屏幕中心)
            this.Left = 100;
            this.Top = 100;

            // 2. 重要：这里不再强制调用 EnableClickThrough
            // 我们让 ViewModel 的默认状态来决定是否穿透
            // 目前 ViewModel 默认为 false (不锁定)，所以这里什么都不用做
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 只有按下左键，且 ViewModel 处于“未锁定”状态时，才允许拖动
            if (e.ChangedButton == MouseButton.Left)
            {
                // 获取当前的数据上下文
                if (DataContext is MainViewModel vm)
                {
                    // 如果没有锁定 (IsOverlayLocked == false)，允许拖拽
                    if (!vm.IsOverlayLocked)
                    {
                        // 这是一个系统方法，用于拖动无边框窗口
                        this.DragMove();
                    }
                }
            }
        }
    }
}