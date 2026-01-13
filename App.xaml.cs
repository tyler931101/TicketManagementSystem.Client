using System.Windows;
using TicketManagementSystem.Client.Services;
using TicketManagementSystem.Client.Views;

namespace TicketManagementSystem.Client
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
