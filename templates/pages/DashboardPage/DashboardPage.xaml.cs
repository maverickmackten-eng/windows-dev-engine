using System.Windows.Controls;
using Serilog;

namespace __APP_NAME__.Views.Pages
{
    /// <summary>
    /// Dashboard page. DataContext = DashboardViewModel (set by NavigationService).
    /// Code-behind is intentionally minimal — all logic lives in ViewModel.
    /// </summary>
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            Log.Debug("[DashboardPage] Initialized");
        }
    }
}
