using System.Windows;
using System.Windows.Input;
using TS6_SpeakerOverlay.ViewModels;

namespace TS6_SpeakerOverlay.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public SettingsWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveConfig();
            this.Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.SaveConfig(); 
            this.Close();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // [修复] 显式指定 System.Windows.MessageBox 以消除歧义
            var result = System.Windows.MessageBox.Show(
                "确定要重置所有设置吗？\n这将恢复默认外观和行为。", 
                "重置确认", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Question
            );

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                _viewModel.Config.ResetDefaults();
            }
        }
    }
}