using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace PopupModalDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            KeyDown += (_, e) => { if (e.Key == Key.Escape) CloseAll(); };
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        // ── Slide Panel ───────────────────────────────────────────
        private void BtnSlidePanel_Click(object sender, RoutedEventArgs e)
        {
            slidePanelHost.Visibility = Visibility.Visible;
            var anim = new DoubleAnimation(350, 0, TimeSpan.FromMilliseconds(320))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            slidePanelTranslate.BeginAnimation(TranslateTransform.XProperty, anim);
            txtStatus.Text = "Slide panel opened — click overlay or X to close";
        }

        private async void SlidePanel_Close(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation(0, 350, TimeSpan.FromMilliseconds(260))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn } };
            var tcs = new TaskCompletionSource<bool>();
            anim.Completed += (_, _) => tcs.SetResult(true);
            slidePanelTranslate.BeginAnimation(TranslateTransform.XProperty, anim);
            await tcs.Task;
            slidePanelHost.Visibility = Visibility.Collapsed;
            txtStatus.Text = "Slide panel closed";
        }

        private void Overlay_Close(object sender, MouseButtonEventArgs e)
            => SlidePanel_Close(sender, new RoutedEventArgs());

        // ── Center Modal ──────────────────────────────────────────
        private void BtnCenterModal_Click(object sender, RoutedEventArgs e)
        {
            centerModalHost.Visibility = Visibility.Visible;
            centerModalHost.Opacity = 0;
            var fadeIn  = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            var scaleX  = new DoubleAnimation(0.85, 1.0, TimeSpan.FromMilliseconds(250))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            var scaleY  = new DoubleAnimation(0.85, 1.0, TimeSpan.FromMilliseconds(250))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            centerModalHost.BeginAnimation(OpacityProperty, fadeIn);
            modalScale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX);
            modalScale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY);
            txtStatus.Text = "Center modal opened — press ESC or click X to close";
        }

        private void CenterModal_Close(object sender, RoutedEventArgs e)
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(180));
            fadeOut.Completed += (_, _) => centerModalHost.Visibility = Visibility.Collapsed;
            centerModalHost.BeginAnimation(OpacityProperty, fadeOut);
            txtStatus.Text = "Center modal closed";
        }

        // ── Confirm Box ───────────────────────────────────────────
        private void BtnConfirmBox_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Delete this item? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            txtStatus.Text = result == MessageBoxResult.Yes
                ? "Confirmed! Item deleted."
                : "Cancelled. Nothing changed.";
        }

        private void CloseAll()
        {
            slidePanelHost.Visibility = Visibility.Collapsed;
            centerModalHost.Visibility = Visibility.Collapsed;
        }
    }
}
