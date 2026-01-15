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
    public partial class RegisterViewModel : ObservableObject
    {
        [ObservableProperty] private string _username = string.Empty;
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
        private async Task Register()
        {
            // Validate email format
            if (string.IsNullOrWhiteSpace(Email) || !IsValidEmail(Email))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please fill all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;

            try
            {
                var response = await _authService.RegisterAsync(new RegisterRequest { Username = Username, Password = Password, Email = Email });

                if (response != null && response.Success)
                {
                    MessageBox.Show("Registration successful! Please login.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Navigate to Login Page directly (not within LayoutPage)
                    var mainWindow = Application.Current.MainWindow;
                    if (mainWindow != null)
                    {
                        var mainFrame = mainWindow.FindName("MainFrame") as Frame;
                        if (mainFrame != null)
                        {
                            mainFrame.Navigate(new LoginPage());
                        }
                    }
                }
                else
                {
                    // Show actual server error message
                    var errorMessage = response?.Message ?? "Registration failed. Please try again.";
                    if (response?.Errors != null && response.Errors.Count > 0)
                    {
                        errorMessage = string.Join("\n", response.Errors);
                    }
                    MessageBox.Show(errorMessage, "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("Unable to connect to server. Please ensure server is running.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NavigateToLogin()
        {
            // Navigate to Login Page directly (not within LayoutPage)
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                var mainFrame = mainWindow.FindName("MainFrame") as Frame;
                if (mainFrame != null)
                {
                    mainFrame.Navigate(new LoginPage());
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
