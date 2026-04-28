using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DropdownMenuDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        private void ComboStyled_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboStyled.SelectedItem is ComboBoxItem item)
                comboResult.Text = $"Selected: {item.Content}";
        }

        private void BtnPopupTrigger_Click(object sender, RoutedEventArgs e)
            => popupMenu.IsOpen = !popupMenu.IsOpen;

        private void BtnCascadeTrigger_Click(object sender, RoutedEventArgs e)
            => popupCascade.IsOpen = !popupCascade.IsOpen;

        private void BtnOpenSub_MouseEnter(object sender, MouseEventArgs e)
            => popupSub.IsOpen = true;

        private void PopupItem_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as Button)?.Tag?.ToString() ?? "?";
            popupResult.Text = $"Clicked: {tag}";
            cascadeResult.Text = $"Clicked: {tag}";
            popupMenu.IsOpen    = false;
            popupCascade.IsOpen = false;
            popupSub.IsOpen     = false;
        }
    }
}
