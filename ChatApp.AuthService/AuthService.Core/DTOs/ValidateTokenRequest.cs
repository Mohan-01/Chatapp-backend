namespace AuthService.Core.DTOs
{
    public class ValidateTokenRequest
    {
        required public string Token { get; set; } = null!;
    }
}
