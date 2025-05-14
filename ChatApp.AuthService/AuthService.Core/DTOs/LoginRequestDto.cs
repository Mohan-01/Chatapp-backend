namespace AuthService.Core.DTOs
{
    public class LoginRequestDto
    {
        required public string Username { get; set; } = null!;
        required public string Password { get; set; } = null!;
    }
}