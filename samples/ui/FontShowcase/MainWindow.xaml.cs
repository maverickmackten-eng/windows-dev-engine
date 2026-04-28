using System.Windows;
using System.Windows.Input;

namespace FontShowcase
{
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
