using System.Windows.Controls;
using Serilog;

namespace __APP_NAME__.Views.Pages
{
    public partial class EmptyPage : UserControl
    {
        public EmptyPage() { InitializeComponent(); Log.Debug("[EmptyPage] Initialized"); }
    }
}
