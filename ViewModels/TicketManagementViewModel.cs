using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TicketManagementSystem.Client.Models;
using TicketManagementSystem.Client.Services;
using TicketManagementSystem.Client.Views;
using TicketManagementSystem.Client.Shared.Controls;
using TicketManagementSystem.Client.Shared.Dialogs;

namespace TicketManagementSystem.Client.ViewModels
{
    public partial class TicketManagementViewModel : ObservableObject
    {
        private readonly ApiService _apiService = new();

        [ObservableProperty]
        private ObservableCollection<Ticket> _tickets = new();

        [ObservableProperty]
        private Ticket? _selectedTicket;

        [ObservableProperty]
        private bool _isLoading = false;

        public TicketManagementViewModel()
        {
            // Initialize with empty collection
            Tickets = new ObservableCollection<Ticket>();
        }

        public async Task LoadTicketsAsync()
        {
            IsLoading = true;
            try
            {
                var currentUser = AuthenticationService.CurrentUser;
                if (currentUser != null)
                {
                    var tickets = await _apiService.GetUserTicketsAsync(currentUser.Username);
                    Tickets.Clear();
                    foreach (var ticket in tickets)
                    {
                        Tickets.Add(ticket);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Failed to load tickets. Please check your connection to the server.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task AddTicket()
        {
            var dialog = new TicketDialog();
            var owner = Application.Current.MainWindow;
            dialog.Owner = owner;
            
            if (dialog.ShowDialog() == true)
            {
                IsLoading = true;
                try
                {
                    var createdTicket = dialog.CreatedOrUpdatedTicket;
                    if (createdTicket != null)
                    {
                        var success = await _apiService.CreateTicketAsync(createdTicket);
                        if (success)
                        {
                            MessageBox.Show("Ticket created successfully!", "Success", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadTicketsAsync();
                        }
                        else
                        {
                            MessageBox.Show("Failed to create ticket.", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to create ticket. Please check your connection to the server.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        [RelayCommand]
        private async Task UpdateTicket()
        {
            if (SelectedTicket == null) return;

            IsLoading = true;
            try
            {
                var success = await _apiService.UpdateTicketAsync(SelectedTicket);
                if (success)
                {
                    MessageBox.Show("Ticket updated successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadTicketsAsync();
                }
                else
                {
                    MessageBox.Show("Failed to update ticket.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("Failed to update ticket. Please check your connection to the server.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void UpdateTicketFromDialog(Ticket updatedTicket)
        {
            var existingTicket = Tickets.FirstOrDefault(t => t.Id == updatedTicket.Id);
            if (existingTicket != null)
            {
                existingTicket.Title = updatedTicket.Title;
                existingTicket.Description = updatedTicket.Description;
                existingTicket.Status = updatedTicket.Status;
                existingTicket.DueDate = updatedTicket.DueDate;
                existingTicket.UpdatedAt = updatedTicket.UpdatedAt;
            }
        }
    }
}
