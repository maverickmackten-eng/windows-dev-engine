using System;
using System.Windows;
using System.Windows.Controls;

namespace __APP_NAME__.Controls
{
    /// <summary>
    /// Full-window loading overlay with spinner and optional cancel button.
    ///
    /// USAGE:
    ///   // Show with message
    ///   loadingOverlay.Show("Scanning files...");
    ///   loadingOverlay.Show("Uploading...", detail: "This may take a moment", showCancel: true);
    ///
    ///   // Hide when done
    ///   loadingOverlay.Hide();
    ///
    ///   // Handle cancel
    ///   loadingOverlay.CancelRequested += (s, e) => _cts.Cancel();
    /// </summary>
    public partial class LoadingOverlay : UserControl
    {
        public event EventHandler? CancelRequested;

        public LoadingOverlay()
        {
            InitializeComponent();
        }

        public void Show(string message = "Loading...", string? detail = null, bool showCancel = false)
        {
            txtLoadingMessage.Text = message;

            if (!string.IsNullOrEmpty(detail))
            {
                txtLoadingDetail.Text = detail;
                txtLoadingDetail.Visibility = Visibility.Visible;
            }
            else
            {
                txtLoadingDetail.Visibility = Visibility.Collapsed;
            }

            btnCancel.Visibility = showCancel ? Visibility.Visible : Visibility.Collapsed;
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
            txtLoadingDetail.Visibility = Visibility.Collapsed;
            btnCancel.Visibility = Visibility.Collapsed;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
            Hide();
        }
    }
}
