using System.Windows;
using System.Windows.Controls;
using Serilog;

namespace DropdownMenuDemo
{
    public partial class MainWindow : Window
    {
        public MainWindow() { InitializeComponent(); Log.Debug("[DropdownMenuDemo] Loaded"); }

        private void CloseClick(object s, RoutedEventArgs e) => Close();

        private void ComboBox_SelectionChanged(object s, SelectionChangedEventArgs e)
        {
            if (StyledComboBox.SelectedItem is ComboBoxItem item)
            {
                ComboResult.Text = $"Selected: {item.Content}";
                Log.Debug("[ComboBox] {Item}", item.Content);
            }
        }

        private void PopupMenuTrigger_Click(object s, RoutedEventArgs e)
            => PopupMenu.IsOpen = !PopupMenu.IsOpen;

        private void MenuItem_NewFile(object s, RoutedEventArgs e)  { PopupMenu.IsOpen = false; Set(PopupResult, "New File"); }
        private void MenuItem_Open(object s, RoutedEventArgs e)      { PopupMenu.IsOpen = false; Set(PopupResult, "Open..."); }
        private void MenuItem_Delete(object s, RoutedEventArgs e)    { PopupMenu.IsOpen = false; Set(PopupResult, "Delete"); }

        private void CascadeTrigger_Click(object s, RoutedEventArgs e)
            => CascadePopup.IsOpen = !CascadePopup.IsOpen;

        private void MoreFormats_MouseEnter(object s, System.Windows.Input.MouseEventArgs e)
            => SubMenu.IsOpen = true;

        private void Export_CSV(object s, RoutedEventArgs e)  { CascadePopup.IsOpen = false; Set(CascadeResult, "CSV"); }
        private void Export_JSON(object s, RoutedEventArgs e) { CascadePopup.IsOpen = false; Set(CascadeResult, "JSON"); }
        private void Export_XML(object s, RoutedEventArgs e)  { CascadePopup.IsOpen = SubMenu.IsOpen = false; Set(CascadeResult, "XML"); }
        private void Export_PDF(object s, RoutedEventArgs e)  { CascadePopup.IsOpen = SubMenu.IsOpen = false; Set(CascadeResult, "PDF"); }
        private void Export_XLSX(object s, RoutedEventArgs e) { CascadePopup.IsOpen = SubMenu.IsOpen = false; Set(CascadeResult, "XLSX"); }

        private void Set(TextBlock tb, string text) { tb.Text = $"✓ {text}"; Log.Debug("[Demo] {Text}", text); }
    }
}
