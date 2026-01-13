using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using TicketManagementSystem.Client.Models;
using TicketManagementSystem.Client.Services;
using TicketManagementSystem.Client.ViewModels;
using TicketManagementSystem.Client.Shared.Controls;
using TicketManagementSystem.Client.Shared.Dialogs;

namespace TicketManagementSystem.Client.Views
{
    public partial class TicketManagementPage : Page
    {
        private TicketManagementViewModel? _viewModel;

        public TicketManagementPage()
        {
            InitializeComponent();
            _viewModel = new TicketManagementViewModel();
            DataContext = _viewModel;
            Loaded += TicketManagementPage_Loaded;
        }

        private async void TicketManagementPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                await _viewModel.LoadTicketsAsync();
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_viewModel?.SelectedTicket != null)
            {
                var dialog = new TicketDialog(_viewModel.SelectedTicket);
                var owner = System.Windows.Application.Current.MainWindow;
                dialog.Owner = owner;
                
                if (dialog.ShowDialog() == true)
                {
                    var updatedTicket = dialog.CreatedOrUpdatedTicket;
                    if (updatedTicket != null)
                    {
                        _viewModel.UpdateTicketFromDialog(updatedTicket);
                    }
                }
            }
        }
    }
}
