using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using TicketManagementSystem.Client.Models;

namespace TicketManagementSystem.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5000/api";

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TicketManagementClient");
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/login", 
                    new { Email = email, Password = password });
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    if (result?.user != null)
                    {
                        AuthenticationService.SetCurrentUser(new User
                        {
                            Id = result.user.Id,
                            Username = result.user.Username,
                            Email = result.user.Email,
                            Role = result.user.Role,
                            IsLoginAllowed = result.user.IsLoginAllowed
                        });
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RegisterAsync(string username, string password, string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/register", 
                    new { Username = username, Email = email, Password = password });
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    // Read error response from server
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetRegistrationErrorAsync(string username, string password, string email)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/register", 
                    new { Username = username, Email = email, Password = password });
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return errorContent ?? "Registration failed";
                }
                
                return string.Empty;
            }
            catch
            {
                return "Connection error";
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/users");
                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<List<User>>();
                    return users ?? new List<User>();
                }
            }
            catch
            {
                // Handle exception
            }
            return new List<User>();
        }

        public async Task<bool> ToggleUserLoginAsync(int userId, bool isAllowed)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/users/{userId}/toggle-login", 
                    new { IsAllowed = isAllowed });
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Ticket>> GetTicketsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/tickets");
                if (response.IsSuccessStatusCode)
                {
                    var tickets = await response.Content.ReadFromJsonAsync<List<TicketDto>>();
                    return tickets?.Select(t => new Ticket
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status,
                        DueDate = t.DueDate,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        AssignedUserId = t.AssignedUserId,
                        AssignedUser = t.AssignedUser != null ? new User
                        {
                            Id = t.AssignedUser.Id,
                            Username = t.AssignedUser.Username,
                        } : null
                    }).ToList() ?? new List<Ticket>();
                }
            }
            catch
            {
                // Handle exception
            }
            return new List<Ticket>();
        }

        public async Task<List<Ticket>> GetUserTicketsAsync(string username)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/tickets/user/{username}");
                if (response.IsSuccessStatusCode)
                {
                    var tickets = await response.Content.ReadFromJsonAsync<List<TicketDto>>();
                    return tickets?.Select(t => new Ticket
                    {
                        Id = t.Id,
                        Title = t.Title,
                        Description = t.Description,
                        Status = t.Status,
                        DueDate = t.DueDate,
                        CreatedAt = t.CreatedAt,
                        UpdatedAt = t.UpdatedAt,
                        AssignedUserId = t.AssignedUserId,
                        AssignedUser = t.AssignedUser != null ? new User
                        {
                            Id = t.AssignedUser.Id,
                            Username = t.AssignedUser.Username,
                        } : null
                    }).ToList() ?? new List<Ticket>();
                }
            }
            catch
            {
                // Handle exception
            }
            return new List<Ticket>();
        }

        public async Task<bool> CreateTicketAsync(Ticket ticket)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/tickets", 
                    new 
                    { 
                        Title = ticket.Title, 
                        Description = ticket.Description, 
                        Status = ticket.Status, 
                        DueDate = ticket.DueDate, 
                        AssignedUserId = ticket.AssignedUserId 
                    });
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/tickets/{ticket.Id}", 
                    new 
                    { 
                        Title = ticket.Title, 
                        Description = ticket.Description, 
                        Status = ticket.Status, 
                        DueDate = ticket.DueDate, 
                        AssignedUserId = ticket.AssignedUserId 
                    });
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteTicketAsync(int ticketId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/tickets/{ticketId}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }

    public class LoginResponse
    {
        public UserDto? user { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsLoginAllowed { get; set; }
    }

    public class TicketDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? AssignedUserId { get; set; }
        public UserDto? AssignedUser { get; set; }
    }
}
