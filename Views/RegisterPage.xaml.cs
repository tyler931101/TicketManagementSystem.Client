using System.Windows;
using System.Windows.Controls;
using TicketManagementSystem.Client.ViewModels;

namespace TicketManagementSystem.Client.Views
{
    public partial class RegisterPage : Page
    {
        private RegisterViewModel? _viewModel;

        public RegisterPage()
        {
            InitializeComponent();
            _viewModel = DataContext as RegisterViewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null && sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }
    }
}
