using System.Net.Http;
using System.Net.Http.Json;
using TicketManagementSystem.Client.DTOs.Auth;
using TicketManagementSystem.Client.DTOs.Common;
using TicketManagementSystem.Client.Models;

namespace TicketManagementSystem.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5000/api";

        public AuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TicketManagementClient");
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/login", loginRequest);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
                
                if (result != null && result.Success && result.Data != null)
                {
                    AuthenticationService.SetCurrentUser(new User
                    {
                        Id = result.Data.User.Id.ToString(),
                        Username = result.Data.User.Username,
                        Email = result.Data.User.Email,
                        Role = result.Data.User.Role,
                        IsLoginAllowed = result.Data.User.IsLoginAllowed
                    }, result.Data.Token);
                }
                
                return result ?? ApiResponse<AuthResponse>.ErrorResult("Login failed");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<object>> RegisterAsync(RegisterRequest registerRequest)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/register", registerRequest);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                return result ?? ApiResponse<object>.ErrorResult("Registration failed");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
        }
    }
}
