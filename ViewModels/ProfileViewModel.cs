using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TicketManagementSystem.Client.DTOs.Common;
using TicketManagementSystem.Client.DTOs.Users;
using TicketManagementSystem.Client.Services;

namespace TicketManagementSystem.Client.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly UserService _userService;
        public sealed class AvatarUploadRequest
        {
            public byte[] AvatarBytes { get; init; } = Array.Empty<byte>();
            public string FileName { get; init; } = string.Empty;
            public string ContentType { get; init; } = "image/png";
        }

        public ProfileViewModel()
        {
            _userService = new UserService();
        }

        [ObservableProperty]
        private UserDto? _profile;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        [ObservableProperty]
        private string _address = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _requiresLogin = false;

        [ObservableProperty]
        private string _currentPassword = string.Empty;

        [ObservableProperty]
        private string _newPassword = string.Empty;

        [ObservableProperty]
        private string _confirmNewPassword = string.Empty;

        [ObservableProperty]
        private ImageSource? _avatarImage;

        [ObservableProperty]
        private ImageSource? _previewAvatarImage;

        [ObservableProperty]
        private string _avatarFileName = string.Empty;

        [RelayCommand]
        private async Task LoadProfileAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                var response = await _userService.GetMeAsync();
                if (response != null && response.Success && response.Data != null)
                {
                    Profile = response.Data;
                    Username = response.Data.Username;
                    Email = response.Data.Email;
                    PhoneNumber = response.Data.PhoneNumber ?? string.Empty;
                    Address = response.Data.Address;
                    StatusMessage = "Profile retrieved";
                    await LoadAvatarAsync();
                }
                else
                {
                    if (response != null && response.StatusCode == 401)
                    {
                        AuthenticationService.Logout();
                        RequiresLogin = true;
                        StatusMessage = "Login required";
                    }
                    else
                    {
                        StatusMessage = "Failed to retrieve profile";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAvatarAsync()
        {
            try
            {
                var id = Profile?.Id;
                if (string.IsNullOrWhiteSpace(id)) return;
                var avatarResp = await _userService.GetAvatarAsync(id);
                if (avatarResp != null && avatarResp.Success && avatarResp.Data != null)
                {
                    using var ms = new MemoryStream(avatarResp.Data);
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();
                    AvatarImage = bmp;
                }
            }
            catch
            {
            }
        }

        [RelayCommand]
        private async Task UpdateProfileAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                var req = new UpdateUserDto
                {
                    Username = string.IsNullOrWhiteSpace(Username) ? null : Username,
                    Email = string.IsNullOrWhiteSpace(Email) ? null : Email,
                    PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber,
                    Address = string.IsNullOrWhiteSpace(Address) ? null : Address
                };
                var response = await _userService.UpdateMeAsync(req);
                if (response != null && response.Success)
                {
                    await LoadProfileAsync();
                    StatusMessage = "Profile updated";
                }
                else
                {
                    if (response != null && response.StatusCode == 401)
                    {
                        AuthenticationService.Logout();
                        RequiresLogin = true;
                        StatusMessage = "Login required";
                    }
                    else
                    {
                        StatusMessage = "Failed to update profile";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ChangePasswordAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                var req = new ChangePasswordDto
                {
                    CurrentPassword = CurrentPassword,
                    NewPassword = NewPassword,
                    ConfirmNewPassword = ConfirmNewPassword
                };
                var response = await _userService.ChangeMyPasswordAsync(req);
                if (response != null && response.Success)
                {
                    StatusMessage = "Password changed";
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmNewPassword = string.Empty;
                }
                else
                {
                    if (response != null && response.StatusCode == 401)
                    {
                        AuthenticationService.Logout();
                        RequiresLogin = true;
                        StatusMessage = "Login required";
                    }
                    else
                    {
                        StatusMessage = "Failed to change password";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task UploadAvatarAsync(AvatarUploadRequest req)
        {
            if (IsLoading) return;
            IsLoading = true;
            try
            {
                if (req.AvatarBytes == null || req.AvatarBytes.Length == 0)
                {
                    StatusMessage = "Please select an image file";
                    return;
                }
                if (req.AvatarBytes.Length > 5 * 1024 * 1024)
                {
                    StatusMessage = "Image too large. Maximum size is 5 MB";
                    return;
                }
                if (!req.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    StatusMessage = "Invalid file type. Please select an image";
                    return;
                }

                using var ms = new MemoryStream(req.AvatarBytes);
                var response = await _userService.UploadMyAvatarAsync(ms, req.FileName, req.ContentType);
                if (response != null && response.Success)
                {
                    StatusMessage = "Avatar uploaded";
                    PreviewAvatarImage = null;
                    AvatarFileName = string.Empty;
                    await LoadAvatarAsync();
                }
                else
                {
                    if (response != null && response.StatusCode == 401)
                    {
                        AuthenticationService.Logout();
                        RequiresLogin = true;
                        StatusMessage = "Login required";
                    }
                    else
                    {
                        StatusMessage = "Failed to upload avatar";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
