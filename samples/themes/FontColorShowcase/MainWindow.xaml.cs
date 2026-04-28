using System.Windows;
using System.Windows.Input;

namespace FontColorShowcase;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        MouseLeftButtonDown += (_, _) => DragMove();
        KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
}
