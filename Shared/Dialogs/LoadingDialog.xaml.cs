using System.Windows;

namespace TicketManagementSystem.Client.Shared.Dialogs
{
    public partial class LoadingDialog : Window
    {
        public LoadingDialog(string message = "Loading...")
        {
            InitializeComponent();
            MessageText.Text = message;
        }

        public void UpdateMessage(string message)
        {
            MessageText.Text = message;
        }
    }
}
