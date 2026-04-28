using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace __APP_NAME__.Controls
{
    /// <summary>
    /// Individual toast notification control.
    /// Managed by ToastService — do not instantiate directly.
    ///
    /// TOAST TYPES (set AccentColor + IconGlyph via ToastItem model):
    ///   Info    — #00B4FF  — &#xE946;
    ///   Success — #00FF94  — &#xE73E;
    ///   Warning — #FFD700  — &#xE7BA;
    ///   Error   — #FF2D55  — &#xEA39;
    /// </summary>
    public partial class ToastNotification : UserControl
    {
        public static readonly int DefaultDisplayMs = 4000;

        private Action? _onDismissed;

        public ToastNotification()
        {
            InitializeComponent();
        }

        /// <summary>Slide in from below and fade in, then auto-dismiss after delay.</summary>
        public async Task ShowAsync(int displayMs = 0, Action? onDismissed = null)
        {
            _onDismissed = onDismissed;
            if (displayMs <= 0) displayMs = DefaultDisplayMs;

            // Animate in: slide up + fade in
            var slideIn = new DoubleAnimation(80, 0, TimeSpan.FromMilliseconds(300))
            { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
            var fadeIn  = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));

            slideTransform.BeginAnimation(TranslateTransform.YProperty, slideIn);
            toastBorder.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(displayMs);
            await DismissAsync();
        }

        public async Task DismissAsync()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            var tcs = new TaskCompletionSource<bool>();
            fadeOut.Completed += (_, _) => tcs.SetResult(true);
            toastBorder.BeginAnimation(OpacityProperty, fadeOut);
            await tcs.Task;
            _onDismissed?.Invoke();
        }

        private async void BtnClose_Click(object sender, RoutedEventArgs e)
            => await DismissAsync();
    }

    // ─────────────────────────────────────────────────────────────────
    // Toast data model
    // ─────────────────────────────────────────────────────────────────
    public enum ToastType { Info, Success, Warning, Error }

    public class ToastItem
    {
        public string Title   { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public ToastType Type { get; set; } = ToastType.Info;
        public int DisplayMs  { get; set; } = ToastNotification.DefaultDisplayMs;

        public string AccentColor => Type switch
        {
            ToastType.Success => "#00FF94",
            ToastType.Warning => "#FFD700",
            ToastType.Error   => "#FF2D55",
            _                 => "#00B4FF"   // Info
        };

        public string IconGlyph => Type switch
        {
            ToastType.Success => "\uE73E",  // Checkmark
            ToastType.Warning => "\uE7BA",  // Warning
            ToastType.Error   => "\uEA39",  // Error
            _                 => "\uE946"   // Info
        };
    }
}
