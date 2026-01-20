using System.Threading.Tasks;
using System.Windows.Controls;
using TicketManagementSystem.Client.ViewModels;

namespace TicketManagementSystem.Client.Views
{
    public partial class AdminDashboardPage : Page
    {
        private readonly AdminDashboardViewModel _viewModel;
        
        public AdminDashboardViewModel ViewModel => _viewModel;
        
        public AdminDashboardPage()
        {
            InitializeComponent();
            _viewModel = new AdminDashboardViewModel();
            DataContext = _viewModel;  // Set DataContext directly
            LoadData();
        }
        
        private async void LoadData()
        {
            // Add a small delay to ensure the UI is fully loaded before executing commands
            await Task.Delay(100);
            await _viewModel.LoadUsersAsync();
        }
    }
}