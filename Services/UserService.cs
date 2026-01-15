using System.Net.Http;
using System.Net.Http.Json;
using TicketManagementSystem.Client.DTOs.Users;
using TicketManagementSystem.Client.DTOs.Common;

namespace TicketManagementSystem.Client.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5000/api";

        public UserService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TicketManagementClient");
        }

        public async Task<ApiResponse<List<UserDto>>> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/users");
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<UserDto>>>();
                return result ?? ApiResponse<List<UserDto>>.ErrorResult("Failed to fetch users");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserDto>>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
        }

        public async Task<bool> ToggleUserLoginAsync(string userId, bool isAllowed)
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
    }
}
