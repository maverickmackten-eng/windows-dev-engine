using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DropdownMenuDemo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
    private void MinBtn_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
    private void NotifBtn_Click(object sender, RoutedEventArgs e) => notifPopup.IsOpen = !notifPopup.IsOpen;
    private void UserBtn_Click(object sender, RoutedEventArgs e) => userPopup.IsOpen = !userPopup.IsOpen;
}
