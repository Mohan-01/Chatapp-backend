namespace AuthService.Core.DTOs
{
    public class ChangeUsernameRequestDto
    {
        required public string NewUsername { get; set; } = null!;
    }
}
