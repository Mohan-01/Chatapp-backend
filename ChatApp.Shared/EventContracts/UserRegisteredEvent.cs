namespace Shared.EventContracts
{
    public class UserRegisteredEvent
    {
        required public string UserId { get; set; } = null!;
        required public string Username { get; set; } = null!;
        required public string Email { get; set; } = null!;
        required public List<string> Roles { get; set; } = []!;
        required public bool IsActive { get; set; } = true;
        required public DateTime CreatedAt { get; set; }
    }
}
