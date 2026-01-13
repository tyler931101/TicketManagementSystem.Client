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
            
            // Load users from database
            LoadUsersAsync();

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
                
                // Set assigned user - delay selection until users are loaded
                this.Loaded += (s, e) =>
                {
                    var selectedUser = AssignedUserComboBox.Items.Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Tag?.ToString() == ticket.AssignedTo);
                    if (selectedUser != null)
                        AssignedUserComboBox.SelectedItem = selectedUser;
                };

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
        }

        private async void LoadUsersAsync()
        {
            try
            {
                var apiService = new ApiService();
                var users = await apiService.GetUsersAsync();
                
                // Clear existing items except "Unassigned"
                var unassignedItem = AssignedUserComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault();
                AssignedUserComboBox.Items.Clear();
                if (unassignedItem != null)
                    AssignedUserComboBox.Items.Add(unassignedItem);
                
                // Add users to ComboBox
                foreach (var user in users)
                {
                    var item = new ComboBoxItem
                    {
                        Content = user.Username,
                        Tag = user.Username
                    };
                    AssignedUserComboBox.Items.Add(item);
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
                existing.AssignedTo = selectedUser?.Tag?.ToString() ?? "";

                CreatedOrUpdatedTicket = existing;
            }
            else
            {
                // Create new ticket
                var selectedPriority = PriorityComboBox.SelectedItem as ComboBoxItem;
                var selectedUser = AssignedUserComboBox.SelectedItem as ComboBoxItem;
                CreatedOrUpdatedTicket = new Ticket
                {
                    Title = TitleBox.Text,
                    Description = DescriptionBox.Text,
                    DueDate = DueDatePicker.SelectedDate ?? DateTime.Now,
                    Status = "To Do",
                    Priority = selectedPriority?.Tag?.ToString() ?? "Medium",
                    AssignedTo = selectedUser?.Tag?.ToString() ?? "",
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