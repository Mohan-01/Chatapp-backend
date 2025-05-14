namespace AuthService.Core.DTOs
{
    public class ResetPasswordRequestDto
    {
        required public string ResetToken { get; set; } = null!;
        required public string NewPassword { get; set; } = null!;
    }
}