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

        private void PrepareRequest()
        {
            if (!string.IsNullOrEmpty(AuthenticationService.CurrentToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthenticationService.CurrentToken);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<ApiResponse<List<TicketDto>>> GetTicketsAsync()
        {
            try
            {
                PrepareRequest();
                var response = await _httpClient.GetAsync($"{BaseUrl}/tickets");
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TicketDto>>>();
                if (result != null) result.StatusCode = (int)response.StatusCode;
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
                PrepareRequest();
                var response = await _httpClient.GetAsync($"{BaseUrl}/tickets/user/{username}");
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TicketDto>>>();
                if (result != null) result.StatusCode = (int)response.StatusCode;
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
                PrepareRequest();
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
                // Create a request object that matches what the server expects
                var serverRequest = new
                {
                    Title = updateTicketDto.Title,
                    Description = updateTicketDto.Description,
                    Status = updateTicketDto.Status,
                    DueDate = updateTicketDto.DueDate,
                    AssignedUserId = updateTicketDto.AssignedUserId != null ? Guid.Parse(updateTicketDto.AssignedUserId) : (Guid?)null
                    // Note: Priority is not sent because server doesn't support it in update
                };
                
                // Debug: Log the request details
                Console.WriteLine($"Updating ticket {ticketId} with data: Title={serverRequest.Title}, Status={serverRequest.Status}");
                
                PrepareRequest();
                var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/tickets/{ticketId}", serverRequest);
                
                Console.WriteLine($"Response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    // Server returns a simple success message, not a TicketDto
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Update successful: {content}");
                    var ok = ApiResponse<TicketDto>.SuccessResult(new TicketDto());
                    ok.StatusCode = (int)response.StatusCode;
                    return ok;
                }
                else
                {
                    // Try to get error details from response
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Update failed with status {response.StatusCode}. Content: {errorContent}");
                    var err = ApiResponse<TicketDto>.ErrorResult($"Update failed: {response.StatusCode}", 
                        new List<string> { errorContent });
                    err.StatusCode = (int)response.StatusCode;
                    return err;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during update: {ex.Message}");
                return ApiResponse<TicketDto>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<object>> DeleteTicketAsync(string ticketId)
        {
            try
            {
                Console.WriteLine($"Attempting to delete ticket {ticketId}");
                
                PrepareRequest();
                var response = await _httpClient.DeleteAsync($"{BaseUrl}/tickets/{ticketId}");
                
                Console.WriteLine($"Delete response status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    // Server returns a simple success message
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Delete successful: {content}");
                    var ok = ApiResponse<object>.SuccessResult(new { }, "Ticket deleted successfully");
                    ok.StatusCode = (int)response.StatusCode;
                    return ok;
                }
                else
                {
                    // Try to get error details from response
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Delete failed with status {response.StatusCode}. Content: {errorContent}");
                    var err = ApiResponse<object>.ErrorResult($"Delete failed: {response.StatusCode}", 
                        new List<string> { errorContent });
                    err.StatusCode = (int)response.StatusCode;
                    return err;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during delete: {ex.Message}");
                return ApiResponse<object>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
        }
    }
}
