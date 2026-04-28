using System;
using System.Windows;
using System.Windows.Media.Animation;
using Serilog;

namespace __APP_NAME__.Views.Controls
{
    public partial class ConfirmDialog : Window
    {
        private bool _result;

        public ConfirmDialog(string title, string? message = null,
                             string confirmText = "Confirm", string cancelText = "Cancel")
        {
            InitializeComponent();

            TitleText.Text  = title;
            ConfirmButton.Content = confirmText;
            CancelButton.Content  = cancelText;

            if (!string.IsNullOrEmpty(message))
            {
                MessageText.Text       = message;
                MessageText.Visibility = Visibility.Visible;
            }

            Loaded += (_, _) => ((Storyboard)FindResource("ScaleIn")).Begin();
            Log.Debug("[ConfirmDialog] Shown: {Title}", title);
        }

        /// <summary>
        /// Shows the dialog modally and returns true if user confirmed.
        /// </summary>
        public bool ShowDialogResult()
        {
            ShowDialog();
            return _result;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _result = true;
            Log.Debug("[ConfirmDialog] Confirmed");
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _result = false;
            Log.Debug("[ConfirmDialog] Cancelled");
            Close();
        }
    }
}
