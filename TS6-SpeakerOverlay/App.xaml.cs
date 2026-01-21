using System.Text;
using System.Windows;

namespace TS6_SpeakerOverlay;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // 设置控制台输出编码为 UTF-8，修复中文乱码
        Console.OutputEncoding = Encoding.UTF8;

        base.OnStartup(e);
    }
}

