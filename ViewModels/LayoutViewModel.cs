using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using TicketManagementSystem.Client.Services;
using TicketManagementSystem.Client.Views;

namespace TicketManagementSystem.Client.ViewModels
{
    public partial class LayoutViewModel : ObservableObject
    {
        private readonly Frame _frame;

        [ObservableProperty]
        private bool _isAdmin;

        [ObservableProperty]
        private bool _isLoggedIn;

        [ObservableProperty]
        private string _currentUsername = string.Empty;

        [ObservableProperty]
        private string _currentRole = string.Empty;

        [ObservableProperty]
        private string _avatarPath = "/Assets/default-avatar.png";

        public LayoutViewModel(Frame frame)
        {
            _frame = frame;
            UpdateLoginState();
            
            // Navigate based on login state
            if (IsLoggedIn)
            {
                _frame.Navigate(new TicketManagementPage());
            }
            else
            {
                // Don't navigate to any page when not logged in
                // Let the user click Login in the header
            }
        }

        private void UpdateLoginState()
        {
            var user = AuthenticationService.CurrentUser;
            IsLoggedIn = user != null;

            if (user != null)
            {
                IsAdmin = user.Role == "Admin";
                CurrentUsername = user.Username;
                CurrentRole = user.Role;
                AvatarPath = string.IsNullOrEmpty(user.AvatarPath) ? "/Assets/default-avatar.png" : user.AvatarPath;
            }
            else
            {
                IsAdmin = false;
                CurrentUsername = string.Empty;
                CurrentRole = string.Empty;
                AvatarPath = "/Assets/default-avatar.png";
            }
        }

        [RelayCommand]
        private void NavigateLogin() =>
            _frame.Navigate(new LoginPage());

        [RelayCommand]
        private void NavigateTickets() =>
            _frame.Navigate(new TicketManagementPage());

        [RelayCommand]
        private void NavigateCharts() =>
            _frame.Navigate(new ChartPage());

        [RelayCommand]
        private void NavigateProfile() =>
            _frame.Navigate(new ProfilePage());

        [RelayCommand]
        private void NavigateDashboard()
        {
            if (!IsAdmin)
            {
                System.Windows.MessageBox.Show("Access denied. Admins only.", "Access Denied", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            _frame.Navigate(new AdminDashboardPage());
        }

        [RelayCommand]
        private void Logout()
        {
            AuthenticationService.Logout();
            UpdateLoginState();
            
            // Navigate to LoginPage (not within LayoutPage)
            var mainWindow = System.Windows.Application.Current.MainWindow;
            if (mainWindow != null)
            {
                var mainFrame = mainWindow.FindName("MainFrame") as Frame;
                if (mainFrame != null)
                {
                    mainFrame.Navigate(new LoginPage());
                }
            }
            
            System.Windows.MessageBox.Show("You have been logged out.", "Logged Out", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}
