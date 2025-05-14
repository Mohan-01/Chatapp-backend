namespace AuthService.Core.DTOs
{
    public class ForgotPasswordRequestDto
    {
        required public string Email { get; set; } = null!;
    }
}