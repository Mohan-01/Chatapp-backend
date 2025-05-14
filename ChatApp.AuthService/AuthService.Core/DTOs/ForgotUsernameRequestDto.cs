namespace AuthService.Core.DTOs
{
    public class ForgotUsernameRequestDto
    {
        required public string Email { get; set; } = null!;
    }
}