using System.ComponentModel.DataAnnotations;

namespace Shared.Models.User
{
    public class SearchUserDto
    {
        required public string Username { get; set; } = null!;
        required public string FirstName { get; set; } = null!;
        required public string LastName { get; set; } = null!;
        required public string ProfilePicture { get; set; } = string.Empty;
        required public string Status { get; set; } = null!;
        required public DateTime LastSeen { get; set; }
    }
}
