using System.ComponentModel.DataAnnotations;

namespace TicketManagementSystem.Client.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        public string? AvatarPath { get; set; }
        public bool IsLoginAllowed { get; set; } = true;
    }
}
