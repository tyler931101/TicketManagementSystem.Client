using Microsoft.Win32;
using System.Windows.Controls;
using TicketManagementSystem.Client.ViewModels;
using System.Windows;

namespace TicketManagementSystem.Client.Views
{
    public partial class ProfilePage : Page
    {
        private readonly ProfileViewModel _viewModel;

        public ProfilePage()
        {
            InitializeComponent();
            _viewModel = new ProfileViewModel();
            DataContext = _viewModel;
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox box)
            {
                _viewModel.CurrentPassword = box.Password;
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox box)
            {
                _viewModel.NewPassword = box.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox box)
            {
                _viewModel.ConfirmNewPassword = box.Password;
            }
        }

        private void SelectAvatar_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _viewModel.LoadAvatarPreviewCommand.Execute(openFileDialog.FileName);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.LoadProfileCommand.Execute(null);
            }
        }
    }
}
