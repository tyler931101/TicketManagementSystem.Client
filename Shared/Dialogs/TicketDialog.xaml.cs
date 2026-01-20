using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TicketManagementSystem.Client.Models;
using TicketManagementSystem.Client.Services;

namespace TicketManagementSystem.Client.Shared.Dialogs
{
    public partial class TicketDialog : Window
    {
        public Ticket? CreatedOrUpdatedTicket { get; private set; }
        private readonly bool _isEditMode;

        public TicketDialog(Ticket? ticket = null)
        {
            InitializeComponent();
            
            if (ticket != null)
            {
                // Edit mode
                _isEditMode = true;
                Title = "Edit Ticket";
                CreateButton.Content = "Update"; // Change button text dynamically

                TitleBox.Text = ticket.Title;
                DescriptionBox.Text = ticket.Description;
                DueDatePicker.SelectedDate = ticket.DueDate;
                
                // Set priority
                var priorityItem = PriorityComboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => item.Tag?.ToString() == ticket.Priority);
                if (priorityItem != null)
                    PriorityComboBox.SelectedItem = priorityItem;
                
                // Store original ticket for updating
                Tag = ticket;
            }
            else
            {
                // Create mode
                _isEditMode = false;
                Title = "Create Ticket";
                CreateButton.Content = "Create";
            }
            
            // Load users from database (this will set assigned user if in edit mode)
            LoadUsersAsync();
        }

        private async void LoadUsersAsync()
        {
            try
            {
                var userService = new UserService();
                var response = await userService.GetTicketUsersAsync();
                
                // Clear existing items
                AssignedUserComboBox.Items.Clear();
                
                // Add users to ComboBox if API call was successful
                if (response != null && response.Success && response.Data != null)
                {
                    foreach (var user in response.Data)
                    {
                        var item = new ComboBoxItem
                        {
                            Content = user.Username,
                            Tag = user.Id.ToString()
                        };
                        AssignedUserComboBox.Items.Add(item);
                    }
                    
                    // Set assigned user if in edit mode (after users are loaded)
                    if (_isEditMode && Tag is Ticket ticket)
                    {
                        // Debug: Log the ticket's assigned user
                        System.Diagnostics.Debug.WriteLine($"Ticket assigned user: {ticket.AssignedTo}");
                        
                        ComboBoxItem? selectedUser = null;
                        
                        // First try to match by ID (if AssignedTo contains user ID)
                        if (!string.IsNullOrEmpty(ticket.AssignedTo))
                        {
                            selectedUser = AssignedUserComboBox.Items.Cast<ComboBoxItem>()
                                .FirstOrDefault(item => item.Tag?.ToString() == ticket.AssignedTo);
                            
                            System.Diagnostics.Debug.WriteLine($"Tried ID match: {selectedUser != null}");
                        }
                        
                        // If ID match failed, try to match by username
                        if (selectedUser == null && !string.IsNullOrEmpty(ticket.AssignedTo))
                        {
                            selectedUser = AssignedUserComboBox.Items.Cast<ComboBoxItem>()
                                .FirstOrDefault(item => item.Content?.ToString() == ticket.AssignedTo);
                            
                            System.Diagnostics.Debug.WriteLine($"Tried username match: {selectedUser != null}");
                        }
                        
                        // Debug: Log available users
                        foreach (ComboBoxItem item in AssignedUserComboBox.Items)
                        {
                            System.Diagnostics.Debug.WriteLine($"Available user: {item.Content} (ID: {item.Tag})");
                        }
                        
                        if (selectedUser != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Found matching user: {selectedUser.Content}");
                            AssignedUserComboBox.SelectedItem = selectedUser;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("No matching user found");
                        }
                    }
                }
            }
            catch
            {
                // Handle error loading users
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text) || DueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please fill Title and Due Date.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isEditMode && Tag is Ticket existing)
            {
                // Update existing ticket
                existing.Title = TitleBox.Text;
                existing.Description = DescriptionBox.Text;
                existing.DueDate = DueDatePicker.SelectedDate ?? DateTime.Now;
                existing.UpdatedAt = DateTime.Now;
                
                // Update priority and assigned user
                var selectedPriority = PriorityComboBox.SelectedItem as ComboBoxItem;
                existing.Priority = selectedPriority?.Tag?.ToString() ?? "Medium";
                
                var selectedUser = AssignedUserComboBox.SelectedItem as ComboBoxItem;
                if (selectedUser?.Tag is string userId)
                    existing.AssignedTo = userId;
                else
                    existing.AssignedTo = null;

                CreatedOrUpdatedTicket = existing;
            }
            else
            {
                // Create new ticket
                var selectedPriority = PriorityComboBox.SelectedItem as ComboBoxItem;
                var selectedUser = AssignedUserComboBox.SelectedItem as ComboBoxItem;
                
                string? assignedUserId = null;
                if (selectedUser?.Tag is string userId)
                    assignedUserId = userId;
                
                CreatedOrUpdatedTicket = new Ticket
                {
                    Title = TitleBox.Text,
                    Description = DescriptionBox.Text,
                    DueDate = DueDatePicker.SelectedDate ?? DateTime.Now,
                    Status = "To Do",
                    Priority = selectedPriority?.Tag?.ToString() ?? "Medium",
                    AssignedTo = assignedUserId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DescriptionBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}