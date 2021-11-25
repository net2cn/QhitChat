using System.Windows;

namespace QhitChat_Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _ = Core.Configuration.Instance;    // Initialize global Configuration instance.
            _ = Presistent.Presistent.Instance;
            Core.Configuration.Network.ConnectAsync(Core.Configuration.HOSTNAME, Core.Configuration.PORT);
        }
    }
}
