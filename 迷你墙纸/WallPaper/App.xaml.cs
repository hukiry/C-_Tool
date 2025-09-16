using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using DMSkin.Wallpaper;
using DMSkin.Wallpaper.API;

namespace 迷你墙纸
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        WindowAPI windowAPI = new WindowAPI();
        protected override void OnStartup(StartupEventArgs e)
        {
            windowAPI.OnStartup(e, () => {
                base.OnStartup(e);
            });
        }
    }
}
