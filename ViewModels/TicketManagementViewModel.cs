using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        private bool _isLoading = false;

        // Search and Filter Properties
        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string? _selectedAssignedUser;

        [ObservableProperty]
        private ObservableCollection<string> _assignedUsers = new();
        
        [ObservableProperty]
        private Dictionary<string, string> _userDisplayMap = new(); // Key: Username, Value: UserId (as string)

        // Kanban board collections
        [ObservableProperty]
        private ObservableCollection<Ticket> _todoTickets = new();

        [ObservableProperty]
        private ObservableCollection<Ticket> _inProgressTickets = new();

        [ObservableProperty]
        private ObservableCollection<Ticket> _testingTickets = new();

        [ObservableProperty]
        private ObservableCollection<Ticket> _resolvedTickets = new();

        [ObservableProperty]
        private ObservableCollection<Ticket> _closedTickets = new();

        // Drag-drop state
        private Ticket? _draggedTicket;

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
                var response = await _ticketService.GetTicketsAsync();
                
                if (response != null && response.Success && response.Data != null)
                {
                    Tickets = new ObservableCollection<Ticket>(response.Data.Select(t => new Ticket
                    {
                        Id = Guid.Parse(t.Id),
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status,
                        DueDate = t.DueDate,
                        Priority = t.Priority,
                        AssignedTo = t.AssignedUserId,
                        AssignedUser = t.AssignedUser != null ? new User
                        {
                            Id = t.AssignedUser.Id,
                            Username = t.AssignedUser.Username,
                            Email = t.AssignedUser.Email
                        } : null,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt
                    }));
                    
                    // Load assigned users for filter
                    await LoadAssignedUsersAsync();
                    
                    // Apply current filters
                    ApplyFilters();
                }
                else
                {
                    MessageBox.Show("Failed to load tickets.", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
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

        private async Task LoadAssignedUsersAsync()
        {
            try
            {
                var userResponse = await _userService.GetTicketUsersAsync();
                if (userResponse != null && userResponse.Success && userResponse.Data != null)
                {
                    var users = userResponse.Data.ToList();
                    var userDisplayList = new List<string> { "All Users" };
                    var userMap = new Dictionary<string, string> { { "All Users", "All Users" } };
                    
                    foreach (var user in users)
                    {
                        if (!string.IsNullOrEmpty(user.Username))
                        {
                            userDisplayList.Add(user.Username);
                            userMap[user.Username] = user.Id.ToString(); // Convert Guid to string
                        }
                    }
                    
                    AssignedUsers = new ObservableCollection<string>(userDisplayList);
                    UserDisplayMap = userMap;
                    SelectedAssignedUser = "All Users";
                }
            }
            catch
            {
                // Handle error silently or show message
            }
        }

        public void ApplyFilters()
        {
            var filteredTickets = Tickets.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filteredTickets = filteredTickets.Where(t => 
                    t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Apply assigned user filter
            if (!string.IsNullOrWhiteSpace(SelectedAssignedUser) && SelectedAssignedUser != "All Users")
            {
                // Get the user ID for the selected username
                if (UserDisplayMap.TryGetValue(SelectedAssignedUser, out string? userId))
                {
                    filteredTickets = filteredTickets.Where(t => 
                        t.AssignedTo == userId);
                }
            }

            // Update Kanban collections
            TodoTickets = new ObservableCollection<Ticket>(filteredTickets.Where(t => t.Status == "To Do"));
            InProgressTickets = new ObservableCollection<Ticket>(filteredTickets.Where(t => t.Status == "In Progress"));
            TestingTickets = new ObservableCollection<Ticket>(filteredTickets.Where(t => t.Status == "Testing"));
            ResolvedTickets = new ObservableCollection<Ticket>(filteredTickets.Where(t => t.Status == "Resolved"));
            ClosedTickets = new ObservableCollection<Ticket>(filteredTickets.Where(t => t.Status == "Closed"));
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedAssignedUser = "All Users";
            ApplyFilters();
        }

        [RelayCommand]
        private async Task LoadTicketsCommand()
        {
            await LoadTicketsAsync();
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
        private async Task EditTicket(Ticket ticket)
        {
            if (ticket == null) return;

            var dialog = new TicketDialog(ticket);
            var owner = Application.Current.MainWindow;
            dialog.Owner = owner;
            
            if (dialog.ShowDialog() == true)
            {
                IsLoading = true;
                try
                {
                    var updatedTicket = dialog.CreatedOrUpdatedTicket;
                    if (updatedTicket != null)
                    {
                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(updatedTicket.Title))
                        {
                            MessageBox.Show("Title is required.", "Validation Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        
                        if (string.IsNullOrWhiteSpace(updatedTicket.Status))
                        {
                            MessageBox.Show("Status is required.", "Validation Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        
                        if (string.IsNullOrWhiteSpace(updatedTicket.Priority))
                        {
                            MessageBox.Show("Priority is required.", "Validation Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        
                        var updateTicketDto = new UpdateTicketDto
                        {
                            Title = updatedTicket.Title,
                            Description = updatedTicket.Description,
                            Status = updatedTicket.Status,
                            DueDate = updatedTicket.DueDate,
                            Priority = updatedTicket.Priority, // This will be sent but server may ignore it
                            AssignedUserId = updatedTicket.AssignedTo
                        };

                        Console.WriteLine($"Sending update request for ticket {updatedTicket.Id}");
                        var response = await _ticketService.UpdateTicketAsync(updatedTicket.Id.ToString(), updateTicketDto);
                        if (response != null && response.Success)
                        {
                            await LoadTicketsAsync();
                            MessageBox.Show("Ticket updated successfully!", "Success", 
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            string errorMessage = "Failed to update ticket.";
                            if (response?.Errors != null && response.Errors.Count > 0)
                            {
                                errorMessage += " " + string.Join("; ", response.Errors);
                            }
                            MessageBox.Show(errorMessage, "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
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
        }

        [RelayCommand]
        private async Task DeleteTicket(Ticket ticket)
        {
            if (ticket == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete ticket '{ticket.Title}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                IsLoading = true;
                try
                {
                    Console.WriteLine($"User confirmed delete for ticket {ticket.Id}");
                    var response = await _ticketService.DeleteTicketAsync(ticket.Id.ToString());
                    if (response != null && response.Success)
                    {
                        await LoadTicketsAsync(); // Reload tickets from server after deletion
                        MessageBox.Show("Ticket deleted successfully!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        string errorMessage = "Failed to delete ticket.";
                        if (response?.Errors != null && response.Errors.Count > 0)
                        {
                            errorMessage += " " + string.Join("; ", response.Errors);
                        }
                        MessageBox.Show(errorMessage, "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch
                {
                    MessageBox.Show("Failed to delete ticket. Please check your connection to the server.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        // Drag and Drop Event Handlers
        public void Ticket_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is Ticket ticket)
            {
                _draggedTicket = ticket;
                DragDrop.DoDragDrop(element, ticket, DragDropEffects.Move);
            }
        }

        public void Todo_Drop(object sender, DragEventArgs e) => HandleDrop(sender, e, "To Do");
        public void InProgress_Drop(object sender, DragEventArgs e) => HandleDrop(sender, e, "In Progress");
        public void Testing_Drop(object sender, DragEventArgs e) => HandleDrop(sender, e, "Testing");
        public void Resolved_Drop(object sender, DragEventArgs e) => HandleDrop(sender, e, "Resolved");
        public void Closed_Drop(object sender, DragEventArgs e) => HandleDrop(sender, e, "Closed");

        public void Todo_DragEnter(object sender, DragEventArgs e) => HandleDragEnter(sender);
        public void InProgress_DragEnter(object sender, DragEventArgs e) => HandleDragEnter(sender);
        public void Testing_DragEnter(object sender, DragEventArgs e) => HandleDragEnter(sender);
        public void Resolved_DragEnter(object sender, DragEventArgs e) => HandleDragEnter(sender);
        public void Closed_DragEnter(object sender, DragEventArgs e) => HandleDragEnter(sender);

        public void Todo_DragLeave(object sender, DragEventArgs e) => HandleDragLeave(sender);
        public void InProgress_DragLeave(object sender, DragEventArgs e) => HandleDragLeave(sender);
        public void Testing_DragLeave(object sender, DragEventArgs e) => HandleDragLeave(sender);
        public void Resolved_DragLeave(object sender, DragEventArgs e) => HandleDragLeave(sender);
        public void Closed_DragLeave(object sender, DragEventArgs e) => HandleDragLeave(sender);

        private void HandleDrop(object sender, DragEventArgs e, string newStatus)
        {
            if (_draggedTicket != null && e.Data.GetData(typeof(Ticket)) is Ticket ticket)
            {
                MoveTicketToStatus(ticket, newStatus);
            }
        }

        private void HandleDragEnter(object sender)
        {
            // No background color changes - keep UI clean
        }

        private void HandleDragLeave(object sender)
        {
            // No background color changes - keep UI clean
        }

        private async void MoveTicketToStatus(Ticket ticket, string newStatus)
        {
            if (ticket.Status == newStatus) return;

            try
            {
                // Remove from current collection
                RemoveTicketFromCollections(ticket);

                // Update status
                ticket.Status = newStatus;
                ticket.UpdatedAt = DateTime.Now;

                // Add to new collection
                AddTicketToCollection(ticket);

                // Try to update on server (but don't show error if it fails)
                try
                {
                    var updateTicketDto = new UpdateTicketDto
                    {
                        Title = ticket.Title,
                        Description = ticket.Description,
                        Status = newStatus,
                        DueDate = ticket.DueDate,
                        Priority = ticket.Priority,
                        AssignedUserId = ticket.AssignedTo
                    };

                    var response = await _ticketService.UpdateTicketAsync(ticket.Id.ToString(), updateTicketDto);
                    if (response == null || !response.Success)
                    {
                        // Server update failed, but UI update succeeded - don't show error to user
                        Console.WriteLine($"Server update failed for ticket {ticket.Id}, but UI was updated successfully");
                    }
                }
                catch (Exception ex)
                {
                    // Server connection failed, but UI update succeeded - don't show error to user
                    Console.WriteLine($"Server connection failed: {ex.Message}, but UI was updated successfully");
                }
            }
            finally
            {
                _draggedTicket = null;
            }
        }

        private void RemoveTicketFromCollections(Ticket ticket)
        {
            TodoTickets.Remove(ticket);
            InProgressTickets.Remove(ticket);
            TestingTickets.Remove(ticket);
            ResolvedTickets.Remove(ticket);
            ClosedTickets.Remove(ticket);
        }

        private void AddTicketToCollection(Ticket ticket)
        {
            switch (ticket.Status.ToLower())
            {
                case "to do":
                    TodoTickets.Add(ticket);
                    break;
                case "in progress":
                    InProgressTickets.Add(ticket);
                    break;
                case "testing":
                    TestingTickets.Add(ticket);
                    break;
                case "resolved":
                    ResolvedTickets.Add(ticket);
                    break;
                case "closed":
                    ClosedTickets.Add(ticket);
                    break;
                default:
                    TodoTickets.Add(ticket);
                    break;
            }
        }
    }
}
