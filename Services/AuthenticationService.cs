using TicketManagementSystem.Client.Models;

namespace TicketManagementSystem.Client.Services
{
    public class AuthenticationService
    {
        public static User? CurrentUser { get; private set; }
        public static string? CurrentToken { get; private set; }

        public static void SetCurrentUser(User user, string? token = null)
        {
            CurrentUser = user;
            if (token != null) CurrentToken = token;
        }

        public static void Logout()
        {
            CurrentUser = null;
            CurrentToken = null;
        }

        public static bool IsAuthenticated => CurrentUser != null;

        public static bool IsAdmin => CurrentUser?.Role == "Admin";
    }
}
