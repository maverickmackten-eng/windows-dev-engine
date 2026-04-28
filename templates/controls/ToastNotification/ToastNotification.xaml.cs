using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Serilog;

namespace __APP_NAME__.Views.Controls
{
    public enum ToastType { Info, Success, Warning, Error }

    public partial class ToastNotification : UserControl
    {
        private static readonly (string Color, string Icon)[] _typeData =
        {
            ("#00B4FF", "\uE946"),  // Info    — blue,  information
            ("#00FF94", "\uE73E"),  // Success — green, checkmark
            ("#FFD700", "\uE7BA"),  // Warning — gold,  warning
            ("#FF2D55", "\uEA39"),  // Error   — red,   error badge
        };

        public ToastNotification() { InitializeComponent(); }

        /// <summary>
        /// Creates, shows, and auto-dismisses a toast. Returns the instance
        /// so the caller can await it or add it to a container.
        /// </summary>
        public static ToastNotification Create(
            string title,
            string? message = null,
            ToastType type = ToastType.Info,
            int autoDismissMs = 4000)
        {
            var toast = new ToastNotification();
            toast.Configure(title, message, type);
            _ = toast.ShowAndAutoCloseAsync(autoDismissMs);
            return toast;
        }

        private void Configure(string title, string? message, ToastType type)
        {
            var (color, icon) = _typeData[(int)type];
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));

            TitleText.Text    = title;
            IconGlyph.Text    = icon;
            IconGlyph.Foreground = brush;
            AccentBar.Background = brush;

            if (!string.IsNullOrEmpty(message))
            {
                MessageText.Text       = message;
                MessageText.Visibility = Visibility.Visible;
            }
            Log.Debug("[Toast] [{Type}] {Title}", type, title);
        }

        private async Task ShowAndAutoCloseAsync(int displayMs)
        {
            ((Storyboard)FindResource("SlideIn")).Begin();
            await Task.Delay(displayMs);
            Dismiss();
        }

        private void Dismiss()
        {
            ((Storyboard)FindResource("SlideOut")).Begin();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e) => Dismiss();

        // After slide-out completes, remove from parent
        private void SlideOut_Completed(object? sender, EventArgs e)
        {
            if (Parent is Panel panel)
                panel.Children.Remove(this);
        }
    }
}
