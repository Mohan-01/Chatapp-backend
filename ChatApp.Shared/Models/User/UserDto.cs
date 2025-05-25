using System.ComponentModel.DataAnnotations;

namespace Shared.Models.User
{
    public class UserDto
    {
        required public string Username { get; set; } = null!;
        required public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        required public string LastName { get; set; } = null!;
        required public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        required public string ProfilePicture { get; set; } = string.Empty;
        required public string Status { get; set; } = null!;
        required public DateTime LastSeen { get; set; }
    }
}
