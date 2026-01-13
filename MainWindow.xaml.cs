using System.Windows;
using System.Windows.Controls;
using TicketManagementSystem.Client.Views;

namespace TicketManagementSystem.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new LoginPage());
        }
    }
}
