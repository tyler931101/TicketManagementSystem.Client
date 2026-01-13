using System.Windows;
using System.Windows.Controls;
using TicketManagementSystem.Client.ViewModels;

namespace TicketManagementSystem.Client.Views
{
    public partial class LoginPage : Page
    {
        private LoginViewModel? _viewModel;

        public LoginPage()
        {
            InitializeComponent();
            _viewModel = DataContext as LoginViewModel;
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
