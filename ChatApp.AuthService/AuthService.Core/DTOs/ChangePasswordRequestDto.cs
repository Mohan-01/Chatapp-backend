namespace AuthService.Core.DTOs
{
    public class ChangePasswordRequestDto
    {
        required public string NewPassword { get; set; } = null!;
    }
}
