using System.Windows;

namespace TicketManagementSystem.Client.Shared.Dialogs
{
    public partial class MessageDialog : Window
    {
        public MessageDialog(string title, string message, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.Information)
        {
            InitializeComponent();
            Title = title;
            MessageText.Text = message;
            
            // Set icon based on type
            switch (icon)
            {
                case MessageBoxImage.Error:
                    IconText.Text = "❌";
                    break;
                case MessageBoxImage.Warning:
                    IconText.Text = "⚠️";
                    break;
                case MessageBoxImage.Question:
                    IconText.Text = "❓";
                    break;
                default:
                    IconText.Text = "ℹ️";
                    break;
            }
            
            // Configure buttons
            if (button == MessageBoxButton.OKCancel)
            {
                CancelButton.Visibility = Visibility.Visible;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
