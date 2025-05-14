using System.Text.Json.Serialization;

namespace ChatApp.UserService.Core.RequestDTOs
{
    public class AuthResponseDto
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Token { get; set; }
        required public string Username { get; set; } = null!;
        required public string Email { get; set; } = null!;
        required public List<string> Roles { get; set; } = null!;

    }
}