using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibVLCSharp.Shared;
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
        _libVLC = new LibVLC("--no-video-title-show", "--quiet");
        _mediaPlayer = new MediaPlayer(_libVLC);

        VideoPlayer.Loaded += (s, e) => VideoPlayer.MediaPlayer = _mediaPlayer;

        _mediaPlayer.PositionChanged += (s, e) =>
        {
            if (ProgressBar.IsPointerOver) return;
            Dispatcher.UIThread.Post(() =>
            {
                _isUpdating = true;
                ProgressBar.Value = e.Position * 100;
                _isUpdating = false;
            });
        };

        AddHandler(DragDrop.DragOverEvent, (s, e) =>
        {
            if (e.DataTransfer.Contains(DataFormat.File)) e.DragEffects = DragDropEffects.Copy;
        }, RoutingStrategies.Bubble);

        AddHandler(DragDrop.DropEvent, (s, e) =>
        {
            var files = e.DataTransfer.TryGetFiles();
            if (files != null && files.Any()) PlayFile(files.First().Path.LocalPath);
        }, RoutingStrategies.Bubble);
    }

    private void ProgressBar_ValueChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (!_isUpdating && ProgressBar.IsPointerOver && _mediaPlayer != null)
        {
            _mediaPlayer.Position = (float)(e.NewValue / 100.0);
        }
    }

    public async void PlayFile(string filePath)
    {
        if (_mediaPlayer == null || !File.Exists(filePath)) return;

        FileNameDisplay.Text = Path.GetFileName(filePath);

        await Task.Run(() => { if (_mediaPlayer.IsPlaying) _mediaPlayer.Stop(); });

        _mediaPlayer.Play(new Media(_libVLC!, filePath, FromType.FromPath));
    }
}