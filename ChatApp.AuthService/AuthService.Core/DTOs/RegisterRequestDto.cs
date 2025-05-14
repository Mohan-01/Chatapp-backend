namespace AuthService.Core.DTOs
{
    public class RegisterRequestDto
    {
        required public string Username { get; set; } = null!;
        required public string Email { get; set; } = null!;
        required public string Password { get; set; } = null!;
    }
}