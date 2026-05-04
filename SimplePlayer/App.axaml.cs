using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using SimplePlayer.Views;
using System.Linq;

namespace SimplePlayer;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();

            // 处理启动参数（双击视频或拖拽打开）
            if (desktop.Args?.Length > 0)
            {
                var filePath = desktop.Args[0];
                if (System.IO.File.Exists(filePath))
                {
                    mainWindow.PlayFile(filePath);   // 下面会加这个方法
                }
            }

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}