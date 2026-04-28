using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ToastDemo
{
    public class ToastItem
    {
        public string Title      { get; set; } = "";
        public string Message    { get; set; } = "";
        public string AccentColor { get; set; } = "#00B4FF";
        public string IconGlyph  { get; set; } = "\uE946";
    }

    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<ToastItem> _toasts = new();
        private int _toastCounter;

        public MainWindow()
        {
            InitializeComponent();
            toastHost.ItemsSource = _toasts;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void BtnInfo_Click(object sender, RoutedEventArgs e)
            => ShowToast("Information", "Scan completed successfully.", "#00B4FF", "\uE946");

        private void BtnSuccess_Click(object sender, RoutedEventArgs e)
            => ShowToast("Success", "Files transferred: 1,024 items.", "#00FF94", "\uE73E");

        private void BtnWarning_Click(object sender, RoutedEventArgs e)
            => ShowToast("Warning", "Disk usage above 85% threshold.", "#FFD700", "\uE7BA");

        private void BtnError_Click(object sender, RoutedEventArgs e)
            => ShowToast("Error", "Connection failed. Retrying in 5s.", "#FF2D55", "\uEA39");

        private async void ShowToast(string title, string message, string color, string glyph)
        {
            var item = new ToastItem
            {
                Title       = title,
                Message     = message,
                AccentColor = color,
                IconGlyph   = glyph
            };

            _toasts.Add(item);

            // Auto-dismiss after 4 seconds
            await Task.Delay(4000);

            if (_toasts.Contains(item))
                _toasts.Remove(item);
        }
    }
}
