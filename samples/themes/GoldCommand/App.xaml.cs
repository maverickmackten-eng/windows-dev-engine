using System.Windows;
using Serilog;

namespace GoldCommand
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Debug().CreateLogger();
            Log.Information("GoldCommand sample starting");
            base.OnStartup(e);
        }
    }
}
