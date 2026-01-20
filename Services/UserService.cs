using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System;
using TicketManagementSystem.Client.DTOs.Users;
using TicketManagementSystem.Client.DTOs.Common;
using TicketManagementSystem.Client.Models;

namespace TicketManagementSystem.Client.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5000/api";
        
        // Simple in-memory cache for pagination responses
        private readonly Dictionary<string, (PagedResponseDto<User> response, DateTime cacheTime)> _cache = new();
        private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);

        public UserService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "TicketManagementClient");
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Add timeout
        }

        public async Task<ApiResponse<PagedResponseDto<User>>> GetUsersAsync(PagedRequestDto request)
        {
            try
            {
                // Create cache key
                var cacheKey = $"users_{request.PageNumber}_{request.PageSize}_{request.SearchTerm}";
                
                // Check cache first
                if (_cache.TryGetValue(cacheKey, out var cached) && 
                    DateTime.UtcNow - cached.cacheTime < _cacheDuration)
                {
                    Console.WriteLine($"Cache hit for {cacheKey}");
                    return ApiResponse<PagedResponseDto<User>>.SuccessResult(cached.response, "Users retrieved from cache");
                }

                // Build query string with optimized parameter encoding
                var queryParams = new List<string>
                {
                    $"pageNumber={request.PageNumber}",
                    $"pageSize={request.PageSize}"
                };

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    queryParams.Add($"searchTerm={Uri.EscapeDataString(request.SearchTerm.Trim())}");
                }

                var queryString = string.Join("&", queryParams);
                var url = $"{BaseUrl}/users?{queryString}";

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                var response = await _httpClient.GetAsync(url);
                stopwatch.Stop();
                
                Console.WriteLine($"API Response Status: {response.StatusCode} (took {stopwatch.ElapsedMilliseconds}ms)");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error Content: {errorContent}");
                    return ApiResponse<PagedResponseDto<User>>.ErrorResult($"Server returned {response.StatusCode}: {errorContent}");
                }
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResponseDto<User>>>();
                
                if (result == null)
                {
                    Console.WriteLine("Failed to deserialize server response");
                    return ApiResponse<PagedResponseDto<User>>.ErrorResult("Failed to deserialize server response");
                }
                
                if (!result.Success)
                {
                    Console.WriteLine($"Server returned error: {result.Message}");
                    return ApiResponse<PagedResponseDto<User>>.ErrorResult(result.Message ?? "Server returned error", result.Errors);
                }
                
                // Cache the successful response
                if (result.Data != null)
                {
                    _cache[cacheKey] = (result.Data, DateTime.UtcNow);
                    
                    // Clean old cache entries
                    var oldKeys = _cache.Keys.Where(k => DateTime.UtcNow - _cache[k].cacheTime > _cacheDuration).ToList();
                    foreach (var oldKey in oldKeys)
                    {
                        _cache.Remove(oldKey);
                    }
                }
                
                Console.WriteLine($"API Success: {typeof(PagedResponseDto<User>).Name}");
                return result;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Error: {ex.Message}");
                return ApiResponse<PagedResponseDto<User>>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"Request Timeout: {ex.Message}");
                return ApiResponse<PagedResponseDto<User>>.ErrorResult("Request timeout", new List<string> { ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return ApiResponse<PagedResponseDto<User>>.ErrorResult("Unexpected error", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<List<TicketUserDto>>> GetTicketUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/users/ticket-users");
                
                Console.WriteLine($"API Response Status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error Content: {errorContent}");
                    return ApiResponse<List<TicketUserDto>>.ErrorResult($"Server returned {response.StatusCode}: {errorContent}");
                }
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TicketUserDto>>>();
                
                if (result == null)
                {
                    Console.WriteLine("Failed to deserialize server response");
                    return ApiResponse<List<TicketUserDto>>.ErrorResult("Failed to deserialize server response");
                }
                
                if (!result.Success)
                {
                    Console.WriteLine($"Server returned error: {result.Message}");
                    return ApiResponse<List<TicketUserDto>>.ErrorResult(result.Message ?? "Server returned error", result.Errors);
                }
                
                Console.WriteLine($"API Success: {typeof(List<TicketUserDto>).Name}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during GetTicketUsersAsync: {ex.Message}");
                return ApiResponse<List<TicketUserDto>>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
        }

        public async Task<ApiResponse<bool>> ToggleUserLoginAsync(string userId, bool isAllowed)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/users/{userId}/toggle-login", 
                    new { IsAllowed = isAllowed });
                
                Console.WriteLine($"API Response Status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Error Content: {errorContent}");
                    return ApiResponse<bool>.ErrorResult($"Server returned {response.StatusCode}: {errorContent}");
                }
                
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                
                if (result == null)
                {
                    Console.WriteLine("Failed to deserialize server response");
                    return ApiResponse<bool>.ErrorResult("Failed to deserialize server response");
                }
                
                if (!result.Success)
                {
                    Console.WriteLine($"Server returned error: {result.Message}");
                    return ApiResponse<bool>.ErrorResult(result.Message ?? "Server returned error", result.Errors);
                }
                
                Console.WriteLine($"API Success: {typeof(bool).Name}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during ToggleUserLoginAsync: {ex.Message}");
                return ApiResponse<bool>.ErrorResult("Connection error", new List<string> { ex.Message });
            }
        }
    }
}
