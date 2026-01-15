using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using TicketManagementSystem.Client.Models;
using TicketManagementSystem.Client.Services;
using TicketManagementSystem.Client.Shared.Dialogs;
using TicketManagementSystem.Client.DTOs.Tickets;

namespace TicketManagementSystem.Client.ViewModels
{
    public partial class TicketManagementViewModel : ObservableObject
    {
        private readonly TicketService _ticketService = new();
        private readonly UserService _userService = new();

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
                    var response = await _ticketService.GetUserTicketsAsync(currentUser.Username);
                    Tickets.Clear();
                    if (response != null && response.Success && response.Data != null)
                    {
                        foreach (var ticketDto in response.Data)
                        {
                            Tickets.Add(new Ticket
                            {
                                Id = Guid.Parse(ticketDto.Id),
                                Title = ticketDto.Title,
                                Description = ticketDto.Description,
                                Status = ticketDto.Status,
                                DueDate = ticketDto.DueDate,
                                Priority = ticketDto.Priority,
                                AssignedTo = ticketDto.AssignedUserId,
                                CreatedAt = ticketDto.CreatedAt,
                                UpdatedAt = ticketDto.UpdatedAt
                            });
                        }
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
                        var createTicketDto = new CreateTicketDto
                        {
                            Title = createdTicket.Title,
                            Description = createdTicket.Description,
                            DueDate = createdTicket.DueDate,
                            Priority = createdTicket.Priority,
                            AssignedUserId = createdTicket.AssignedTo
                        };
                        var response = await _ticketService.CreateTicketAsync(createTicketDto);
                        if (response != null && response.Success)
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
                var updateTicketDto = new UpdateTicketDto
                {
                    Title = SelectedTicket.Title,
                    Description = SelectedTicket.Description,
                    Status = SelectedTicket.Status,
                    DueDate = SelectedTicket.DueDate,
                    Priority = SelectedTicket.Priority,
                    AssignedUserId = SelectedTicket.AssignedTo
                };
                var response = await _ticketService.UpdateTicketAsync(SelectedTicket.Id.ToString(), updateTicketDto);
                if (response != null && response.Success)
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
