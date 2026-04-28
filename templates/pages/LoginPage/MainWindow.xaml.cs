using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LoginPage;

public partial class MainWindow : Window
{
    private static readonly SolidColorBrush AccentBrush = new(Color.FromRgb(0x00, 0xB4, 0xFF));
    private static readonly SolidColorBrush BorderBrush_ = new(Color.FromRgb(0x2A, 0x33, 0x47));

    public MainWindow()
    {
        InitializeComponent();
        KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
    }

    private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e) => DragMove();
    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();

    private void Field_GotFocus(object sender, RoutedEventArgs e) => userBorder.BorderBrush = AccentBrush;
    private void Field_LostFocus(object sender, RoutedEventArgs e) => userBorder.BorderBrush = BorderBrush_;
    private void PassField_GotFocus(object sender, RoutedEventArgs e) => passBorder.BorderBrush = AccentBrush;
    private void PassField_LostFocus(object sender, RoutedEventArgs e) => passBorder.BorderBrush = BorderBrush_;

    private void ShowPass_Click(object sender, RoutedEventArgs e)
    {
        // PasswordBox doesn't support show/hide natively — swap with TextBox in production
    }

    private void SignIn_Click(object sender, RoutedEventArgs e)
    {
        var user = usernameBox.Text.Trim();
        var pass = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            ShowError("Username and password are required.");
            return;
        }

        // Replace with real auth logic
        if (user == "commander.maverick" && pass == "warpspeed")
        {
            errorBorder.Visibility = Visibility.Collapsed;
            // Navigate to main window
            // var main = new MainWindow(); main.Show();
            Close();
        }
        else
        {
            ShowError("Invalid credentials. Try again.");
        }
    }

    private void ShowError(string message)
    {
        errorText.Text = message;
        errorBorder.Visibility = Visibility.Visible;
    }
}
