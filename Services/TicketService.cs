using System.Net.Http;
using System.Net.Http.Json;
using TicketManagementSystem.Client.DTOs.Tickets;
using TicketManagementSystem.Client.DTOs.Common;

namespace TicketManagementSystem.Client.Services
{
    public class TicketService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5000/api";

        public TicketService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TicketManagementClient");
        }

        public async Task<ApiResponse<List<TicketDto>>> GetTicketsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/tickets");
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TicketDto>>>();
                return result ?? ApiResponse<List<TicketDto>>.ErrorResult("Failed to fetch tickets");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TicketDto>>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<List<TicketDto>>> GetUserTicketsAsync(string username)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/tickets/user/{username}");
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TicketDto>>>();
                return result ?? ApiResponse<List<TicketDto>>.ErrorResult("Failed to fetch user tickets");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TicketDto>>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<TicketDto>> CreateTicketAsync(CreateTicketDto createTicketDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/tickets", createTicketDto);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<TicketDto>>();
                return result ?? ApiResponse<TicketDto>.ErrorResult("Failed to create ticket");
            }
            catch (Exception ex)
            {
                return ApiResponse<TicketDto>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<TicketDto>> UpdateTicketAsync(string ticketId, UpdateTicketDto updateTicketDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/tickets/{ticketId}", updateTicketDto);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<TicketDto>>();
                return result ?? ApiResponse<TicketDto>.ErrorResult("Failed to update ticket");
            }
            catch (Exception ex)
            {
                return ApiResponse<TicketDto>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<object>> DeleteTicketAsync(string ticketId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/tickets/{ticketId}");
                
                if (response.IsSuccessStatusCode)
                {
                    return ApiResponse<object>.SuccessResult(new { }, "Ticket deleted successfully");
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                return ApiResponse<object>.ErrorResult("Failed to delete ticket", new List<string> { errorContent });
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
        }
    }
}
