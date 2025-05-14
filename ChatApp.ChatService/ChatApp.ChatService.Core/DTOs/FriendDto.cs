namespace ChatApp.ChatService.Core.DTOs
{
    public class FriendDto
    {
        public string Username { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? ProfilePicture { get; set; } = string.Empty;
        public string UserStatus { get; set; } = null!;
        public string FriendshipStatus { get; set; } = null!;
        public DateTime LastSeen { get; set; }
    }

}
