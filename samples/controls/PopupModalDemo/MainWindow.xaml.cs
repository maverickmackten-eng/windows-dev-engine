using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Serilog;

namespace PopupModalDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow() { InitializeComponent(); Log.Debug("[PopupModalDemo] Loaded"); }

        private void CloseClick(object s, RoutedEventArgs e) => Close();

        // ---- Pattern 1: Confirm dialog ----
        private void OpenConfirm_Click(object s, RoutedEventArgs e)
        {
            ConfirmOverlay.Visibility = Visibility.Visible;
            ((Storyboard)FindResource("ModalScaleIn")).Begin();
            ConfirmOverlay.Focus();
        }

        private void ConfirmOK_Click(object s, RoutedEventArgs e)
        {
            ConfirmOverlay.Visibility = Visibility.Collapsed;
            ConfirmResult.Text = "✓ Confirmed";
            ConfirmResult.Foreground = System.Windows.Media.Brushes.LimeGreen;
            Log.Debug("[Confirm] OK");
        }

        private void ConfirmCancel_Click(object s, RoutedEventArgs e)
        {
            ConfirmOverlay.Visibility = Visibility.Collapsed;
            ConfirmResult.Text = "× Cancelled";
            ConfirmResult.Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x88, 0x99, 0xAA));
            Log.Debug("[Confirm] Cancelled");
        }

        private void Overlay_KeyDown(object s, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) ConfirmCancel_Click(s, new RoutedEventArgs());
        }

        // ---- Pattern 2: Form modal ----
        private void OpenForm_Click(object s, RoutedEventArgs e)
        {
            NameField.Clear();
            NotesField.Clear();
            NameError.Visibility = Visibility.Collapsed;
            FormOverlay.Visibility = Visibility.Visible;
            NameField.Focus();
            Log.Debug("[Form] Opened");
        }

        private void FormCancel_Click(object s, RoutedEventArgs e)
        {
            FormOverlay.Visibility = Visibility.Collapsed;
            FormResult.Text = "× Cancelled";
            Log.Debug("[Form] Cancelled");
        }

        private void FormSave_Click(object s, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameField.Text))
            {
                NameError.Visibility = Visibility.Visible;
                NameField.Focus();
                return;
            }
            var category = (CategoryField.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "";
            FormOverlay.Visibility = Visibility.Collapsed;
            FormResult.Text = $"✓ Name: {NameField.Text}\nCat: {category}";
            Log.Information("[Form] Saved: {Name} / {Cat}", NameField.Text, category);
        }

        private void NameField_TextChanged(object s, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (NameField.Text.Length > 0)
                NameError.Visibility = Visibility.Collapsed;
        }

        // ---- Pattern 3: Drawer ----
        private void OpenDrawer_Click(object s, RoutedEventArgs e)
        {
            DrawerOverlay.Visibility = Visibility.Visible;
            ((Storyboard)FindResource("DrawerSlideIn")).Begin();
            Log.Debug("[Drawer] Opened");
        }

        private void CloseDrawer_Click(object s, RoutedEventArgs e)
            => ((Storyboard)FindResource("DrawerSlideOut")).Begin();

        private void DrawerBackdrop_MouseDown(object s, MouseButtonEventArgs e)
            => ((Storyboard)FindResource("DrawerSlideOut")).Begin();

        private void DrawerSlideOut_Completed(object? s, System.EventArgs e)
        {
            DrawerOverlay.Visibility = Visibility.Collapsed;
            Log.Debug("[Drawer] Closed");
        }

        private void DrawerEdit_Click(object s, RoutedEventArgs e)
        {
            ((Storyboard)FindResource("DrawerSlideOut")).Begin();
            Log.Debug("[Drawer] Edit clicked");
        }

        private void DrawerDelete_Click(object s, RoutedEventArgs e)
        {
            ((Storyboard)FindResource("DrawerSlideOut")).Begin();
            Log.Debug("[Drawer] Delete clicked");
        }
    }
}
