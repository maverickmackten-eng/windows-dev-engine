using System.Windows;
using System.Windows.Input;

namespace MaverickMach10;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
        MouseLeftButtonDown += (_, _) => DragMove();
    }
}
