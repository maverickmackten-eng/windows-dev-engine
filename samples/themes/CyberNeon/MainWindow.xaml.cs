using System.Windows;
using Serilog;

namespace CyberNeon
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Log.Debug("[CyberNeon] MainWindow loaded");
        }

        private void MinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void CloseClick(object sender, RoutedEventArgs e) => Close();
    }
}
