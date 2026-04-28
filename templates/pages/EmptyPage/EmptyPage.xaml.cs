using System.Windows.Controls;
using Serilog;

namespace __APP_NAME__.Views.Pages
{
    /// <summary>
    /// Blank page scaffold. Copy and rename for each new page.
    /// DataContext set by NavigationService.
    /// </summary>
    public partial class EmptyPage : Page
    {
        public EmptyPage()
        {
            InitializeComponent();
            Log.Debug("[EmptyPage] Initialized — replace this with your page name");
        }
    }
}
