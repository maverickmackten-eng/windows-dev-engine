using System.Windows.Controls;

namespace __APP_NAME__.Debug
{
    /// <summary>
    /// Code-behind for DiagnosticsOverlay.
    /// DataContext is DiagnosticsViewModel — set it in the parent window:
    ///
    ///   var vm = new DiagnosticsViewModel();
    ///   diagOverlay.DataContext = vm;
    ///
    /// Toggle visibility via MainWindow keyboard shortcut Ctrl+Shift+D.
    /// </summary>
    public partial class DiagnosticsOverlay : UserControl
    {
        public DiagnosticsOverlay()
        {
            InitializeComponent();
        }
    }
}
