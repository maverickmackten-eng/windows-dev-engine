using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Serilog;

namespace __APP_NAME__.Views.Controls
{
    public partial class LoadingOverlay : UserControl
    {
        private readonly Storyboard _spin;
        private readonly Storyboard _fadeIn;

        public LoadingOverlay()
        {
            InitializeComponent();
            _spin   = (Storyboard)FindResource("SpinForever");
            _fadeIn = (Storyboard)FindResource("FadeIn");
        }

        /// <summary>Show the overlay with an optional message.</summary>
        public void Show(string message = "Loading...")
        {
            MessageText.Text = message;
            Visibility       = Visibility.Visible;
            _spin.Begin();
            _fadeIn.Begin();
            Log.Debug("[LoadingOverlay] Shown: {Message}", message);
        }

        /// <summary>Hide the overlay and stop the spinner.</summary>
        public void Hide()
        {
            _spin.Stop();
            Visibility = Visibility.Collapsed;
            Root.Opacity = 0;
            Log.Debug("[LoadingOverlay] Hidden");
        }
    }
}
