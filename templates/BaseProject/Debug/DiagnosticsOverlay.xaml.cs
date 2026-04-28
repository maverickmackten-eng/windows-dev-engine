using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BaseApp.Debug;

public partial class DiagnosticsOverlay : UserControl
{
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _uptime = Stopwatch.StartNew();
    private DateTime _lastFrameTime = DateTime.UtcNow;
    private int _frameCount;
    private int _fps;

    public string CurrentView { get; set; } = "MainWindow";

    public DiagnosticsOverlay()
    {
        InitializeComponent();

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;

        IsVisibleChanged += (_, e) =>
        {
            if ((bool)e.NewValue)
            {
                _timer.Start();
                CompositionTarget.Rendering += OnRendering;
            }
            else
            {
                _timer.Stop();
                CompositionTarget.Rendering -= OnRendering;
            }
        };
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        _frameCount++;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _fps = _frameCount;
        _frameCount = 0;

        FpsText.Text = _fps.ToString();
        FpsText.Foreground = _fps switch
        {
            >= 55 => new SolidColorBrush(Color.FromRgb(0, 255, 148)),   // green
            >= 30 => new SolidColorBrush(Color.FromRgb(255, 184, 0)),   // yellow
            _ => new SolidColorBrush(Color.FromRgb(255, 45, 85))        // red
        };

        var memMb = GC.GetTotalMemory(false) / 1_048_576.0;
        MemoryText.Text = $"{memMb:F1} MB";

        UptimeText.Text = _uptime.Elapsed.ToString(@"hh\:mm\:ss");

        ViewText.Text = CurrentView;
    }

    public void SetLastLog(string message)
    {
        Dispatcher.InvokeAsync(() => LastLogText.Text = message);
    }
}
