using System.Windows.Controls;
using Serilog;

namespace __APP_NAME__.Views.Pages
{
    /// <summary>
    /// Settings page. DataContext = SettingsViewModel (set by NavigationService).
    /// All settings logic in ViewModel; code-behind is intentionally empty.
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            Log.Debug("[SettingsPage] Initialized");
        }
    }
}
