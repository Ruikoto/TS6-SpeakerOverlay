using System;
using System.Windows; // 这里的 using 其实不够消除歧义，还是得写全名
using System.Windows.Interop;
using System.Windows.Media;

namespace TS6_SpeakerOverlay
{
    // [修复关键] 这里必须明确指定是 System.Windows.Application (WPF的)，而不是 Forms 的
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // 强制软件渲染，解决《猎杀：对决》等游戏不显示的问题
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            base.OnStartup(e);
        }
    }
}