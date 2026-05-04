using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimplePlayer.Views;

public partial class MainWindow : Window
{
    private LibVLC? _libVLC;
    private MediaPlayer? _mediaPlayer;
    private bool _isUpdating = false;

    public MainWindow()
    {
        InitializeComponent();
        Core.Initialize();
        _libVLC = new LibVLC("--no-video-title-show");
        _mediaPlayer = new MediaPlayer(_libVLC);
        VideoPlayer.Loaded += (s, e) => VideoPlayer.MediaPlayer = _mediaPlayer;

        // 视频位置改变时，更新底部进度条
        _mediaPlayer.PositionChanged += (s, e) =>
        {
            Dispatcher.UIThread.Post(() => {
                if (!ProgressBar.IsPointerOver)
                {
                    _isUpdating = true;
                    ProgressBar.Value = e.Position * 100;
                    _isUpdating = false;
                }
            });
        };

        AddHandler(DragDrop.DragOverEvent, OnDragOver, RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
        AddHandler(DragDrop.DropEvent, OnDrop, RoutingStrategies.Bubble | RoutingStrategies.Tunnel);
    }

    // 增加一个私有变量用来记录之前的状态
    private WindowState _previousState = WindowState.Normal;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.Space && _mediaPlayer != null)
        {
            if (_mediaPlayer.IsPlaying) _mediaPlayer.Pause(); else _mediaPlayer.Play();
        }
        else if (e.Key == Key.F)
        {
            if (WindowState == WindowState.FullScreen)
            {
                // 退出全屏：还原到进入前的状态
                WindowState = _previousState;
            }
            else
            {
                // 进入全屏：先记下当前是最大化还是普通窗口，再变身
                _previousState = WindowState;
                WindowState = WindowState.FullScreen;
            }
        }
        else if (e.Key == Key.Escape)
        {
            // 按 Esc 退出也遵循同样的逻辑
            if (WindowState == WindowState.FullScreen)
            {
                WindowState = _previousState;
            }
        }
    }

    private void ProgressBar_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!_isUpdating && _mediaPlayer != null && ProgressBar.IsPointerOver)
        {
            _mediaPlayer.Position = (float)(e.NewValue / 100.0);
        }
    }

    public async void PlayFile(string filePath)
    {
        if (_mediaPlayer == null || !File.Exists(filePath)) return;

        // 直接更新窗口标题，不搞额外的 TextBlock
        this.Title = Path.GetFileName(filePath);

        await Task.Run(() => { if (_mediaPlayer.IsPlaying) _mediaPlayer.Stop(); });
        _mediaPlayer.Play(new Media(_libVLC!, filePath, FromType.FromPath));
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer?.Formats.Contains(DataFormat.File) == true)
        {
            e.DragEffects = DragDropEffects.Copy;
            e.Handled = true;
        }
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer?.TryGetFiles() is { } files && files.Any())
        {
            PlayFile(files.First().Path.LocalPath);
        }
    }
}