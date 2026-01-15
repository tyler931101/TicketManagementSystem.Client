using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using TicketManagementSystem.Client.Services;
using TicketManagementSystem.Client.Views;
using TicketManagementSystem.Client.DTOs.Auth;

namespace TicketManagementSystem.Client.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _password = string.Empty;
        [ObservableProperty] private bool _isLoading = false;

        private readonly AuthService _authService = new();

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Use MailAddress class for proper email validation
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email && !string.IsNullOrWhiteSpace(mailAddress.Host);
            }
            catch
            {
                // If parsing fails, consider it invalid
                return false;
            }
        }

        [RelayCommand]
        private async Task Login()
        {
            // Validate email format
            if (string.IsNullOrWhiteSpace(Email) || !IsValidEmail(Email))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please enter your password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            try
            {
                var response = await _authService.LoginAsync(new LoginRequest { Email = Email, Password = Password });

                if (response == null || !response.Success)
                {
                    MessageBox.Show("Invalid email or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var currentUser = AuthenticationService.CurrentUser;
                if (currentUser != null && !currentUser.IsLoginAllowed)
                {
                    MessageBox.Show("Login is not allowed, please contact support team", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    AuthenticationService.Logout();
                    return;
                }

                MessageBox.Show($"Welcome, {currentUser?.Username}!", "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Navigate to LayoutPage (which will show TicketManagementPage since user is now logged in)
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow != null)
                {
                    var mainFrame = mainWindow.FindName("MainFrame") as Frame;
                    if (mainFrame != null)
                    {
                        mainFrame.Navigate(new LayoutPage());
                    }
                }
            }
            catch
            {
                MessageBox.Show("Unable to connect to server. Please ensure the server is running.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NavigateToRegister()
        {
            // Navigate to RegisterPage directly (not within LayoutPage)
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                var mainFrame = mainWindow.FindName("MainFrame") as Frame;
                if (mainFrame != null)
                {
                    mainFrame.Navigate(new RegisterPage());
                }
            }
        }

        private Frame? FindParentFrame()
        {
            // Try to find the frame that contains this page
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                // First try to find LayoutPage and its ContentFrame (most likely case)
                var layoutPage = mainWindow.Content as LayoutPage;
                if (layoutPage != null)
                {
                    return layoutPage.FindName("ContentFrame") as Frame;
                }
                
                // Fallback: try MainFrame (for standalone pages)
                var mainFrame = mainWindow.FindName("MainFrame") as Frame;
                if (mainFrame != null)
                {
                    return mainFrame;
                }
            }
            return null;
        }
    }
}
