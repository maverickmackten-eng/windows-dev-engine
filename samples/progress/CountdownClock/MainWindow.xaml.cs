using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace CountdownClock;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer = new();
    private TimeSpan _remaining = TimeSpan.FromMinutes(10);
    private TimeSpan _total = TimeSpan.FromMinutes(10);
    private bool _running;

    public MainWindow()
    {
        InitializeComponent();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTick;
        MouseLeftButtonDown += (_, _) => DragMove();
        UpdateDisplay();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _remaining = _remaining.Subtract(TimeSpan.FromSeconds(1));

        if (_remaining <= TimeSpan.Zero)
        {
            _remaining = TimeSpan.Zero;
            _timer.Stop();
            _running = false;
            OnTimerComplete();
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        hoursText.Text = _remaining.Hours.ToString("D2");
        minutesText.Text = _remaining.Minutes.ToString("D2");
        secondsText.Text = _remaining.Seconds.ToString("D2");

        // Progress bar fill
        double progressPct = 1.0 - (_remaining.TotalSeconds / _total.TotalSeconds);
        progressBar.Width = Math.Max(0, (500 - 80) * progressPct); // container width - margin

        // Color changes as time runs out
        var color = _remaining.TotalSeconds switch
        {
            > 300 => Color.FromRgb(0, 180, 255),   // blue: plenty of time
            > 60  => Color.FromRgb(255, 184, 0),   // yellow: getting low
            _     => Color.FromRgb(255, 45, 85)     // red: critical
        };
        var brush = new SolidColorBrush(color);
        hoursText.Foreground = minutesText.Foreground = secondsText.Foreground = brush;

        var glowEffect = new System.Windows.Media.Effects.DropShadowEffect
        {
            Color = color, BlurRadius = _remaining.TotalSeconds < 60 ? 30 : 20,
            ShadowDepth = 0, Opacity = 0.7
        };
        hoursText.Effect = minutesText.Effect = secondsText.Effect = glowEffect;

        statusText.Text = _running ? "COUNTING DOWN" :
                          _remaining == TimeSpan.Zero ? "TIME'S UP" : "PAUSED";
    }

    private void OnTimerComplete()
    {
        statusText.Text = "MISSION LAUNCH — T+0";
        statusText.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 148));
        missionLabel.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 148));
    }

    private void StartBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_running) { _timer.Stop(); _running = false; startBtn.Content = "START"; }
        else { _timer.Start(); _running = true; startBtn.Content = "PAUSE"; }
        UpdateDisplay();
    }

    private void ResetBtn_Click(object sender, RoutedEventArgs e)
    {
        _timer.Stop(); _running = false;
        _remaining = _total;
        startBtn.Content = "START";
        missionLabel.Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x55, 0x66));
        UpdateDisplay();
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
}
