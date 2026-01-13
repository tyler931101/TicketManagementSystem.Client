using TicketManagementSystem.Client.Models;

namespace TicketManagementSystem.Client.Services
{
    public class AuthenticationService
    {
        public static User? CurrentUser { get; private set; }

        public static void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public static void Logout()
        {
            CurrentUser = null;
        }

        public static bool IsAuthenticated => CurrentUser != null;

        public static bool IsAdmin => CurrentUser?.Role == "Admin";
    }
}
