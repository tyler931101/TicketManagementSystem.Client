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

        private void TitleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        public void UpdateMessage(string message)
        {
            MessageText.Text = message;
        }
    }
}
