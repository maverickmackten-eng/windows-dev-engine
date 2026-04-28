using System.Windows.Controls;
using Serilog;

namespace __APP_NAME__.Views.Pages
{
    public partial class SettingsPage : UserControl
    {
        public SettingsPage() { InitializeComponent(); Log.Debug("[SettingsPage] Initialized"); }
    }
}
