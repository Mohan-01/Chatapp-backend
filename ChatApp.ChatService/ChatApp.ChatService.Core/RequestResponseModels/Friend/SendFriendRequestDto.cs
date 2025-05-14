using System.ComponentModel.DataAnnotations;

namespace ChatApp.ChatService.Core.RequestResponseModels.Friend
{
    public class SendFriendRequestDto
    {
        [Required]
        public string RecipientUsername { get; set; } = null!;
    }
}
