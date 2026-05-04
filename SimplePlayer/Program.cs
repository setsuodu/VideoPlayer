using Avalonia;
using System;
using System.Threading;

namespace SimplePlayer;

class Program
{
    private static Mutex? _mutex;
    private const string MutexName = "Global\\SimpleDragPlayer_SingleInstance"; // 全局唯一

    [STAThread]
    public static void Main(string[] args)
    {
        // 单实例检查
        bool isNewInstance;
        _mutex = new Mutex(true, MutexName, out isNewInstance);

        if (!isNewInstance)
        {
            // 已经有实例在跑 → 激活它并退出
            Console.WriteLine("Already running, activating existing instance...");
            // 这里可以扩展通过 NamedPipe 传文件路径给第一个实例（高级点再说）
            return;
        }

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}