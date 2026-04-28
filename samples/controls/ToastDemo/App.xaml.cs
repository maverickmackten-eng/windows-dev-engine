using System.Windows;
using Serilog;

namespace ToastDemo
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Debug().CreateLogger();
            base.OnStartup(e);
        }
    }
}
