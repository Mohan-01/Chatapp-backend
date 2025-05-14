using System.ComponentModel.DataAnnotations;

namespace Shared.Models.User
{
    public class UserDto
    {
        [Required]
        public string Username { get; set; } = null!;
        [Required]
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        [Required]
        public string LastName { get; set; } = null!;
        [Required]
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public List<string> Roles { get; set; } = [];
        public string ProfilePicture { get; set; } = string.Empty;
        [Required]
        public string Status { get; set; } = null!;
        public DateTime LastSeen { get; set; }
    }
}
