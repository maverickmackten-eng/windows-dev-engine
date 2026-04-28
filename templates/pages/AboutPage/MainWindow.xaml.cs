using System.Windows;
using System.Windows.Input;

namespace AboutPage;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
}
