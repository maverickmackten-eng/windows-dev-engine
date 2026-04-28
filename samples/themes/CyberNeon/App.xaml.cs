using System.Windows;
using Serilog;

namespace CyberNeon
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Debug().CreateLogger();
            Log.Information("CyberNeon sample starting");
            base.OnStartup(e);
        }
    }
}
