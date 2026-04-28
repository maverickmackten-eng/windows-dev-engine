using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ButtonGallery;

public partial class MainWindow : Window
{
    private bool _shieldsOn = false;

    public MainWindow()
    {
        InitializeComponent();
        KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
    private void MinBtn_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

    private void ToggleBtn_Click(object sender, RoutedEventArgs e)
    {
        _shieldsOn = !_shieldsOn;
        if (_shieldsOn)
        {
            toggleBtn.Content = "SHIELDS: ON";
            toggleBtn.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 148));
            toggleBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 255, 148));
            toggleBtn.Background = new SolidColorBrush(Color.FromArgb(0x1A, 0, 255, 148));
        }
        else
        {
            toggleBtn.Content = "SHIELDS: OFF";
            toggleBtn.Foreground = new SolidColorBrush(Color.FromRgb(0x44, 0x55, 0x66));
            toggleBtn.BorderBrush = new SolidColorBrush(Color.FromRgb(0x2A, 0x33, 0x47));
            toggleBtn.Background = new SolidColorBrush(Color.FromRgb(0x1C, 0x23, 0x33));
        }
    }
}
