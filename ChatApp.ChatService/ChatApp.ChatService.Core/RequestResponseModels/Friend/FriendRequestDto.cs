using System.ComponentModel.DataAnnotations;

namespace ChatApp.ChatService.Core.RequestResponseModels.Friend
{
    public class FriendRequestDto
    {
        [Required]
        public string Id { get; set; } = null!;
        [Required]
        public string SenderUsername { get; set; } = null!;
        [Required]
        public string SenderFirstName { get; set; } = null!;
        [Required]
        public string SenderLastName { get; set; } = null!;
        public string? SenderProfilePicture { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
