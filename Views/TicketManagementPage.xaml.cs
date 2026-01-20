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

        // Drag and Drop Event Handlers
        private void Ticket_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _viewModel?.Ticket_MouseLeftButtonDown(sender, e);
        }

        // Delete Ticket Event Handler
        private void DeleteTicket_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Ticket ticket)
            {
                _viewModel?.DeleteTicketCommand.Execute(ticket);
            }
        }

        // Edit Ticket Event Handler
        private void EditTicket_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Ticket ticket)
            {
                _viewModel?.EditTicketCommand.Execute(ticket);
            }
        }

        // Search and Filter Event Handlers
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel?.ApplyFilters();
        }

        private void AssignedUserFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel?.ApplyFilters();
        }

        private void Todo_Drop(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.Todo_Drop(sender, e);
        }

        private void Todo_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.Todo_DragEnter(sender, e);
        }

        private void Todo_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.Todo_DragLeave(sender, e);
        }

        private void InProgress_Drop(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.InProgress_Drop(sender, e);
        }

        private void InProgress_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.InProgress_DragEnter(sender, e);
        }

        private void InProgress_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.InProgress_DragLeave(sender, e);
        }

        private void Testing_Drop(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.Testing_Drop(sender, e);
        }

        private void Testing_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.Testing_DragEnter(sender, e);
        }

        private void Testing_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.Testing_DragLeave(sender, e);
        }

        private void Resolved_Drop(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.Resolved_Drop(sender, e);
        }

        private void Resolved_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.Resolved_DragEnter(sender, e);
        }

        private void Resolved_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.Resolved_DragLeave(sender, e);
        }

        private void Closed_Drop(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.Closed_Drop(sender, e);
        }

        private void Closed_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.Closed_DragEnter(sender, e);
        }

        private void Closed_DragLeave(object sender, System.Windows.DragEventArgs e)
        {
            _viewModel?.Closed_DragLeave(sender, e);
        }
    }
}
