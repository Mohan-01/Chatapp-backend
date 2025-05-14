namespace ChatApp.UserService.Core.RequestDTOs
{
    public class ValidateTokenRequest
    {
        required public string Token { get; set; } = null!;
    }
}
