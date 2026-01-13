using System.Windows.Controls;

namespace TicketManagementSystem.Client.Services
{
    public class NavigationService
    {
        private static Frame? _mainFrame;

        public static void Initialize(Frame frame)
        {
            _mainFrame = frame;
        }

        public static void Navigate(Page page)
        {
            _mainFrame?.Navigate(page);
        }

        public static void GoBack()
        {
            if (_mainFrame?.CanGoBack == true)
                _mainFrame.GoBack();
        }
    }
}
