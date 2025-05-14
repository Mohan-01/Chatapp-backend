namespace AuthService.Core.DTOs
{
    public class UpdateEmailRequestDto
    {
        required public string NewEmail { get; set; } = null!;
    }
}
