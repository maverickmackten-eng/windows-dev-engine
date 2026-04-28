using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Serilog;

namespace ToastDemo
{
    public enum ToastType { Info, Success, Warning, Error }

    public partial class MainWindow : Window
    {
        private int    _duration    = 4000;   // ms, 0 = sticky
        private string _position    = "BottomRight";
        private int    _firedCount  = 0;

        private static readonly (Color Accent, Color Text, string Icon, string Label)[] _types =
        {
            (Color.FromRgb(0x00, 0xB4, 0xFF), Colors.White,   "\u2139", "Info"),
            (Color.FromRgb(0x00, 0xC4, 0x7A), Colors.White,   "\u2714", "Success"),
            (Color.FromRgb(0xFF, 0xD7, 0x00), Color.FromRgb(0x0A, 0x0D, 0x12), "\u26a0", "Warning"),
            (Color.FromRgb(0xFF, 0x2D, 0x55), Colors.White,   "\u2716", "Error"),
        };

        public MainWindow() { InitializeComponent(); Log.Debug("[ToastDemo] Loaded"); }

        private void CloseClick(object s, RoutedEventArgs e) => Close();

        // Fire buttons
        private void Fire_Info(object s, RoutedEventArgs e)    => FireToast(ToastType.Info,    "Information", "Operation completed successfully.");
        private void Fire_Success(object s, RoutedEventArgs e) => FireToast(ToastType.Success, "Success!",    "Record saved to database.");
        private void Fire_Warning(object s, RoutedEventArgs e) => FireToast(ToastType.Warning, "Warning",    "Disk space below 10%.");
        private void Fire_Error(object s, RoutedEventArgs e)   => FireToast(ToastType.Error,   "Error",      "Connection refused — retry in 5s.");

        private void FireToast(ToastType type, string title, string message)
        {
            _firedCount++;
            FiredCount.Text = $"Toasts fired: {_firedCount}";

            var (accent, textColor, icon, _) = _types[(int)type];
            var accentBrush = new SolidColorBrush(accent);
            var textBrush   = new SolidColorBrush(textColor);

            // Build toast
            var translate = new TranslateTransform(340, 0);
            var toast = new Border
            {
                Width           = 320,
                Background      = new SolidColorBrush(Color.FromRgb(0x12, 0x17, 0x1F)),
                BorderBrush     = new SolidColorBrush(Color.FromArgb(0x55, 0x2A, 0x33, 0x47)),
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(8),
                Margin          = new Thickness(0, 0, 0, 8),
                Opacity         = 0,
                RenderTransform = translate,
                Effect          = new System.Windows.Media.Effects.DropShadowEffect
                    { BlurRadius = 16, Opacity = 0.4, ShadowDepth = 4 },
                Child = BuildToastContent(icon, title, message, accentBrush, textBrush)
            };

            // Stack: insert at top (BottomRight = first item nearest bottom)
            if (_position == "TopRight")
                ToastHost.Children.Add(toast);
            else
                ToastHost.Children.Insert(0, toast);

            ReflowToasts();

            // Slide + fade in
            var slideIn = new DoubleAnimation(340, 0, TimeSpan.FromMilliseconds(280))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            var fadeIn  = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            translate.BeginAnimation(TranslateTransform.XProperty, slideIn);
            toast.BeginAnimation(OpacityProperty, fadeIn);

            // Auto-dismiss
            if (_duration > 0)
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_duration) };
                timer.Tick += (_, _) => { timer.Stop(); DismissToast(toast); };
                timer.Start();
            }

            Log.Debug("[Toast] Fired {Type}: {Title}", type, title);
        }

        private Grid BuildToastContent(string icon, string title, string message,
                                        SolidColorBrush accentBrush, SolidColorBrush textBrush)
        {
            var grid = new Grid { Margin = new Thickness(0) };
            // Left accent bar
            grid.Children.Add(new Border
            {
                Width = 4, HorizontalAlignment = HorizontalAlignment.Left,
                CornerRadius = new CornerRadius(8, 0, 0, 8), Background = accentBrush
            });
            // Content
            var contentGrid = new Grid { Margin = new Thickness(16, 12, 12, 12) };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Icon
            var iconTb = new TextBlock
            {
                Text = icon, FontSize = 18, Foreground = accentBrush,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(iconTb, 0);

            // Text stack
            var textStack = new StackPanel();
            textStack.Children.Add(new TextBlock
            {
                Text = title, FontSize = 13, FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Colors.White), TextWrapping = TextWrapping.Wrap
            });
            textStack.Children.Add(new TextBlock
            {
                Text = message, FontSize = 12, TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x99, 0xAA)),
                Margin = new Thickness(0, 2, 0, 0)
            });
            Grid.SetColumn(textStack, 1);

            // Close button — captured for dismiss
            Border? toastRef = null;
            var closeBtn = new Button
            {
                Content = "\u00d7", FontSize = 14,
                Background = Brushes.Transparent,
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x99, 0xAA)),
                BorderThickness = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Top, Padding = new Thickness(4)
            };
            closeBtn.Click += (_, _) =>
            {
                // Walk up to the toast Border
                DependencyObject? p = closeBtn;
                while (p is not null and not Border b || (b = (Border)p!) is { CornerRadius: { TopLeft: < 8 } })
                    p = VisualTreeHelper.GetParent(p);
                if (p is Border toastBorder) DismissToast(toastBorder);
            };
            Grid.SetColumn(closeBtn, 2);

            contentGrid.Children.Add(iconTb);
            contentGrid.Children.Add(textStack);
            contentGrid.Children.Add(closeBtn);
            grid.Children.Add(contentGrid);
            return grid;
        }

        private void DismissToast(Border toast)
        {
            var slideOut = new DoubleAnimation(0, 340, TimeSpan.FromMilliseconds(220))
                { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
            var fadeOut  = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(180));
            slideOut.Completed += (_, _) =>
            {
                ToastHost.Children.Remove(toast);
                ReflowToasts();
            };
            ((TranslateTransform)toast.RenderTransform).BeginAnimation(TranslateTransform.XProperty, slideOut);
            toast.BeginAnimation(OpacityProperty, fadeOut);
        }

        // Re-stack toasts vertically from bottom up
        private void ReflowToasts()
        {
            double y = 0;
            for (int i = ToastHost.Children.Count - 1; i >= 0; i--)
            {
                if (ToastHost.Children[i] is not Border t) continue;
                Canvas.SetBottom(t, y);
                Canvas.SetRight(t, 0);
                y += t.ActualHeight + 8;
            }
        }

        // Duration buttons
        private void DurBtn_Click(object s, RoutedEventArgs e)
        {
            _duration = int.Parse(((Button)s).Tag.ToString()!);
            // Reset all to inactive style
            foreach (var b in new[] { Dur2Btn, Dur4Btn, Dur8Btn, DurSticky })
            { b.Background = new SolidColorBrush(Color.FromRgb(0x1C, 0x23, 0x33));
              b.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x99, 0xAA));
              b.BorderBrush = new SolidColorBrush(Color.FromRgb(0x2A, 0x33, 0x47)); }
            // Highlight active
            var active = (Button)s;
            active.Background   = new SolidColorBrush(Color.FromRgb(0x0D, 0x3A, 0x5C));
            active.Foreground   = new SolidColorBrush(Color.FromRgb(0x00, 0xB4, 0xFF));
            active.BorderBrush  = new SolidColorBrush(Color.FromRgb(0x00, 0xB4, 0xFF));
        }

        // Position buttons
        private void PosBtn_Click(object s, RoutedEventArgs e)
        {
            _position = ((Button)s).Tag.ToString()!;
            ToastHost.VerticalAlignment = _position == "TopRight"
                ? VerticalAlignment.Top : VerticalAlignment.Bottom;
            foreach (var b in new[] { PosTopRight, PosBotRight })
            { b.Background = new SolidColorBrush(Color.FromRgb(0x1C, 0x23, 0x33));
              b.Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x99, 0xAA));
              b.BorderBrush = new SolidColorBrush(Color.FromRgb(0x2A, 0x33, 0x47)); }
            var active = (Button)s;
            active.Background  = new SolidColorBrush(Color.FromRgb(0x0D, 0x3A, 0x5C));
            active.Foreground  = new SolidColorBrush(Color.FromRgb(0x00, 0xB4, 0xFF));
            active.BorderBrush = new SolidColorBrush(Color.FromRgb(0x00, 0xB4, 0xFF));
        }
    }
}
