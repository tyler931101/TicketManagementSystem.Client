using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TicketManagementSystem.Client.Services;
using TicketManagementSystem.Client.Views;

namespace TicketManagementSystem.Client.Shared.Controls
{
    public partial class Header : UserControl
    {
        public Header()
        {
            InitializeComponent();
        }

        private void Logo_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Navigate to LayoutPage (which will show appropriate page based on login state)
            var mainWindow = System.Windows.Application.Current.MainWindow;
            if (mainWindow != null)
            {
                var mainFrame = mainWindow.FindName("MainFrame") as Frame;
                if (mainFrame != null)
                {
                    mainFrame.Navigate(new LayoutPage());
                }
            }
        }

        private void ProfileDropdownButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Toggle the dropdown popup
            ProfileDropdownPopup.IsOpen = !ProfileDropdownPopup.IsOpen;
        }
    }
}
