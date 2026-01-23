using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using TicketManagementSystem.Client.ViewModels;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TicketManagementSystem.Client.Views
{
    public partial class ProfilePage : Page
    {
        private ProfileViewModel? _viewModel;
        private byte[]? _lastSelectedAvatarBytes;
        private string _lastSelectedFileName = string.Empty;
        private string _lastSelectedContentType = "application/octet-stream";

        public ProfilePage()
        {
            InitializeComponent();
            _viewModel = new ProfileViewModel();
            DataContext = _viewModel;
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel != null && sender is PasswordBox box)
            {
                _viewModel.CurrentPassword = box.Password;
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel != null && sender is PasswordBox box)
            {
                _viewModel.NewPassword = box.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel != null && sender is PasswordBox box)
            {
                _viewModel.ConfirmNewPassword = box.Password;
            }
        }

        private void UploadAvatar_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            var dialog = new OpenFileDialog
            {
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.gif;*.bmp",
                Multiselect = false
            };
            if (dialog.ShowDialog() == true)
            {
                var path = dialog.FileName;
                var contentType = GetContentType(path);
                var bytes = File.ReadAllBytes(path);
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = fs;
                    bmp.EndInit();
                    bmp.Freeze();
                    _viewModel.PreviewAvatarImage = bmp;
                }
                _viewModel.AvatarFileName = System.IO.Path.GetFileName(path);
                _lastSelectedAvatarBytes = bytes;
                _lastSelectedFileName = System.IO.Path.GetFileName(path);
                _lastSelectedContentType = contentType;
            }
        }

        private void UploadAvatarConfirm_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_viewModel == null) return;
            if (_lastSelectedAvatarBytes == null || _lastSelectedAvatarBytes.Length == 0) return;
            var req = new ProfileViewModel.AvatarUploadRequest
            {
                AvatarBytes = _lastSelectedAvatarBytes,
                FileName = _lastSelectedFileName,
                ContentType = _lastSelectedContentType
            };
            _viewModel.UploadAvatarCommand.Execute(req);
        }

        private static string GetContentType(string path)
        {
            var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".png") return "image/png";
            if (ext == ".jpg" || ext == ".jpeg") return "image/jpeg";
            if (ext == ".gif") return "image/gif";
            if (ext == ".bmp") return "image/bmp";
            return "application/octet-stream";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel?.LoadProfileCommand.Execute(null);
        }
    }
}
