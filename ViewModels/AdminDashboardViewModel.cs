using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TicketManagementSystem.Client.DTOs.Common;
using TicketManagementSystem.Client.DTOs.Users;
using TicketManagementSystem.Client.Models;
using TicketManagementSystem.Client.Services;

namespace TicketManagementSystem.Client.ViewModels
{
    public partial class AdminDashboardViewModel : ObservableObject
    {
        private readonly UserService _userService;
        
        public AdminDashboardViewModel()
        {
            _userService = new UserService();
            
            // Initialize properties to prevent null reference issues
            _filteredUsers = new ObservableCollection<UserDto>();
            _searchText = string.Empty;
            _currentPage = 1;
            _pageSize = 5;
            _totalRecords = 0;
            _startRecord = 0;
            _endRecord = 0;
            _lastPageNumber = 1;
            _isLoading = false;
            _isInitialized = false;
            _pageNumbers = new ObservableCollection<int>();
            _showEllipsis = false;
            _showLastPageButton = false;
        }
        
        [ObservableProperty]
        private ObservableCollection<UserDto> _filteredUsers = new();
        
        [ObservableProperty]
        private string _searchText = string.Empty;
        
        // Pagination Properties
        [ObservableProperty]
        private int _currentPage = 1;
        
        [ObservableProperty]
        private int _pageSize = 10;
        
        partial void OnPageSizeChanged(int value)
        {
            System.Diagnostics.Debug.WriteLine($"PageSize changed to: {value}");
            
            // Reset to first page when page size changes
            CurrentPage = 1;
            _ = LoadUsersAsync();
        }
        
        [ObservableProperty]
        private int _totalRecords = 0;
        
        [ObservableProperty]
        private int _startRecord = 1;
        
        [ObservableProperty]
        private int _endRecord = 10;
        
        [ObservableProperty]
        private int _lastPageNumber = 1;
        
        [ObservableProperty]
        private bool _isLoading = false;
        
        [ObservableProperty]
        private bool _isInitialized = false;
        
        [ObservableProperty]
        private ObservableCollection<int> _pageNumbers = new();
        
        [ObservableProperty]
        private bool _showEllipsis = false;
        
        [ObservableProperty]
        private bool _showLastPageButton = false;
        
        [ObservableProperty]
        private bool _requiresLogin = false;
        
        public async Task LoadUsersAsync()
        {
            System.Diagnostics.Debug.WriteLine($"LoadUsersAsync called - CurrentPage: {CurrentPage}, PageSize: {PageSize}, SearchTerm: '{SearchText}'");
            
            // Cancel any previous loading operation
            _loadingCancellationTokenSource?.Cancel();
            _loadingCancellationTokenSource = new CancellationTokenSource();
            
            await LoadUsersWithPaginationAsync(_loadingCancellationTokenSource.Token);
        }
        
        private CancellationTokenSource? _loadingCancellationTokenSource;
        
        private async Task LoadUsersWithPaginationAsync(CancellationToken cancellationToken = default)
        {
            if (_userService == null || IsLoading)
            {
                return;
            }

            IsLoading = true;
            try
            {
                var request = new PagedRequestDto
                {
                    PageNumber = CurrentPage,
                    PageSize = PageSize,
                    SearchTerm = SearchText?.Trim() ?? string.Empty
                };

                var response = await _userService.GetUsersAsync(request);
                
                if (cancellationToken.IsCancellationRequested)
                {
                    System.Diagnostics.Debug.WriteLine("LoadUsersAsync cancelled");
                    return;
                }
                
                if (response != null && response.Success && response.Data != null)
                {
                    var pagedData = response.Data;
                    
                    // Use efficient object initialization
                    var userDtos = new List<UserDto>(pagedData.Data.Count());
                    var index = 0;
                    foreach (var serverUser in pagedData.Data)
                    {
                        userDtos.Add(new UserDto
                        {
                            RowNumber = (CurrentPage - 1) * PageSize + index + 1,
                            Id = serverUser.Id.ToString(),
                            Username = serverUser.Username,
                            Email = serverUser.Email,
                            Address = serverUser.Address,
                            PhoneNumber = serverUser.PhoneNumber,
                            Role = serverUser.Role,
                            IsLoginAllowed = serverUser.IsLoginAllowed,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        });
                        index++;
                    }

                    // Update UI on UI thread
                    FilteredUsers = new ObservableCollection<UserDto>(userDtos);
                    
                    // Update pagination info from server response
                    UpdatePaginationInfo(pagedData);
                }
                else
                {
                    // Handle error
                    if (response != null && response.StatusCode == 401)
                    {
                        AuthenticationService.Logout();
                        RequiresLogin = true;
                    }
                    ResetPaginationInfo();
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("LoadUsersAsync operation cancelled");
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in LoadUsersWithPaginationAsync: {ex.Message}");
                ResetPaginationInfo();
            }
            finally
            {
                IsLoading = false;
                IsInitialized = true; // Mark as initialized after first successful load
            }
        }
        
        private void UpdatePaginationInfo(PagedResponseDto<User> pagedData)
        {
            CurrentPage = pagedData.CurrentPage;
            TotalRecords = pagedData.TotalRecords;
            LastPageNumber = pagedData.TotalPages;
            
            // Calculate start and end records based on server pagination
            StartRecord = TotalRecords == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
            EndRecord = TotalRecords == 0 ? 0 : Math.Min(CurrentPage * PageSize, TotalRecords);
            
            // Calculate dynamic page numbers based on server response
            CalculatePageNumbers();
        }
        
        private void CalculatePageNumbers()
        {
            var pageNumbers = new List<int>();
            
            if (LastPageNumber <= 7)
            {
                // Show all pages if total pages <= 7
                for (int i = 1; i <= LastPageNumber; i++)
                {
                    pageNumbers.Add(i);
                }
                ShowEllipsis = false;
                ShowLastPageButton = false; // No separate last page button needed
            }
            else
            {
                // Show smart pagination for more than 7 pages
                ShowLastPageButton = true; // Always show last page button for >7 pages
                ShowEllipsis = true; // Always show ellipsis for >7 pages
                
                if (CurrentPage <= 4)
                {
                    // Show first 5 pages (last page shown separately)
                    for (int i = 1; i <= 5; i++)
                    {
                        if (i < LastPageNumber) // Don't include last page
                            pageNumbers.Add(i);
                    }
                }
                else if (CurrentPage >= LastPageNumber - 3)
                {
                    // Show first page + last 4 pages (last page shown separately)
                    pageNumbers.Add(1);
                    var startPage = Math.Max(2, LastPageNumber - 4); // Start from at least page 2
                    for (int i = startPage; i < LastPageNumber; i++) // Exclude last page
                    {
                        pageNumbers.Add(i);
                    }
                }
                else
                {
                    // Show first page + current page +/- 1 (last page shown separately)
                    pageNumbers.Add(1);
                    for (int i = CurrentPage - 1; i <= CurrentPage + 1; i++)
                    {
                        if (i > 1 && i < LastPageNumber) // Don't add first or last page
                            pageNumbers.Add(i);
                    }
                }
            }
            
            // Remove duplicates and sort
            pageNumbers = pageNumbers.Distinct().OrderBy(x => x).ToList();
            PageNumbers = new ObservableCollection<int>(pageNumbers);
        }
        
        private void ResetPaginationInfo()
        {
            FilteredUsers = new ObservableCollection<UserDto>();
            TotalRecords = 0;
            LastPageNumber = 1;
            StartRecord = 0;
            EndRecord = 0;
            PageNumbers = new ObservableCollection<int>();
            ShowEllipsis = false;
            ShowLastPageButton = false;
        }
        
        private async Task NavigateToPage(int pageNumber)
        {
            if (pageNumber >= 1 && pageNumber <= LastPageNumber && !IsLoading && _userService != null)
            {
                CurrentPage = pageNumber;
                await LoadUsersAsync();
            }
        }
        
        [RelayCommand]
        private void PreviousPage() 
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (_userService != null && CurrentPage > 1 && !IsLoading && IsInitialized)
                        await NavigateToPage(CurrentPage - 1);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in PreviousPage command: {ex.Message}");
                }
            });
        }
        
        [RelayCommand]
        private void NextPage() 
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (_userService != null && CurrentPage < LastPageNumber && !IsLoading && IsInitialized)
                        await NavigateToPage(CurrentPage + 1);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in NextPage command: {ex.Message}");
                }
            });
        }
        
        [RelayCommand]
        private void GoToPage(int pageNumber) 
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (_userService != null && pageNumber >= 1 && pageNumber <= LastPageNumber && !IsLoading && IsInitialized)
                        await NavigateToPage(pageNumber);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in GoToPage command: {ex.Message}");
                }
            });
        }
        
        [RelayCommand]
        private void GoToLastPage() 
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (_userService != null && LastPageNumber >= 1 && !IsLoading && IsInitialized)
                        await NavigateToPage(LastPageNumber);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in GoToLastPage command: {ex.Message}");
                }
            });
        }
        
        [RelayCommand]
        private void ClearFilters()
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    if (_userService != null && !IsLoading && IsInitialized)
                    {
                        SearchText = string.Empty;
                        CurrentPage = 1;
                        await LoadUsersAsync();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in ClearFilters command: {ex.Message}");
                }
            });
        }
        
        private CancellationTokenSource? _searchCancellationTokenSource;
        
        partial void OnSearchTextChanged(string value)
        {
            // Cancel previous search
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();
            
            // Optimized debounce - shorter delay for better UX
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(200, _searchCancellationTokenSource.Token);
                    if (!_searchCancellationTokenSource.Token.IsCancellationRequested && SearchText == value)
                    {
                        CurrentPage = 1;
                        await LoadUsersAsync();
                    }
                }
                catch (TaskCanceledException)
                {
                    // Search was cancelled, ignore
                }
            }, _searchCancellationTokenSource.Token);
        }
    }
}
