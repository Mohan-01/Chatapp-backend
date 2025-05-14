namespace AuthService.Core.DTOs
{
    public class UpdateEmailResponseDto
    {
        required public string NewEmail { get; set; } = null!;
        required public string Message { get; set; } = null!;
    }
}
