namespace Shared.DTOs
{
    public class ValidateTokenResponseDto
    {
        required public string Username { get; set; } = null!;
        required public string Email { get; set; } = null!;
        required public List<string> Roles { get; set; } = null!;
    }

}
