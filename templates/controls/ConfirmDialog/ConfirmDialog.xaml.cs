using System.Windows;
using System.Windows.Input;

namespace __APP_NAME__.Controls
{
    /// <summary>
    /// Modal confirm dialog with YES/NO result.
    ///
    /// USAGE:
    ///   var dialog = new ConfirmDialog(this)  // pass owner window
    ///   {
    ///       TitleText   = "Delete Item",
    ///       MessageText = "Delete \"config.json\"? This cannot be undone.",
    ///       ConfirmText = "DELETE",
    ///       IsDangerous = true  // makes YES button red instead of blue
    ///   };
    ///   bool confirmed = dialog.ShowAndGetResult();
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        public bool Result { get; private set; }

        public string TitleText
        {
            get => txtTitle.Text;
            set => txtTitle.Text = value;
        }

        public string MessageText
        {
            get => txtMessage.Text;
            set => txtMessage.Text = value;
        }

        public string ConfirmText
        {
            get => (string)btnYes.Content;
            set => btnYes.Content = value;
        }

        public bool IsDangerous
        {
            set
            {
                if (value)
                {
                    btnYes.Background = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2D55"));
                    btnYes.Foreground = System.Windows.Media.Brushes.White;
                    txtIcon.Text = "\uEA39"; // Error icon
                    txtIcon.Foreground = new System.Windows.Media.SolidColorBrush(
                        (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF2D55"));
                }
            }
        }

        public ConfirmDialog(Window owner)
        {
            InitializeComponent();
            Owner = owner;

            // Close on ESC
            KeyDown += (_, e) => { if (e.Key == Key.Escape) { Result = false; Close(); } };
        }

        /// <summary>Shows the dialog and returns true if user clicked Confirm.</summary>
        public bool ShowAndGetResult()
        {
            ShowDialog();
            return Result;
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            Close();
        }
    }
}
