using System;
using System.Windows.Controls;
using TicketManagementSystem.Client.ViewModels;

namespace TicketManagementSystem.Client.Views
{
    public partial class LayoutPage : Page
    {
        public LayoutPage()
        {
            InitializeComponent();
            try
            {
                DataContext = new LayoutViewModel(ContentFrame);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in LayoutPage constructor: {ex.Message}");
                // Fallback: don't navigate to any page
            }
        }

        private void AvatarButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // This is handled by the ContextMenu
        }
    }
}
