using System.Windows.Controls;
using Serilog;

namespace __APP_NAME__.Views.Pages
{
    public partial class DashboardPage : UserControl
    {
        public DashboardPage() { InitializeComponent(); Log.Debug("[DashboardPage] Initialized"); }
    }
}
