using System.ComponentModel.DataAnnotations;

namespace ChatApp.ChatService.Core.RequestResponseModels.Friend
{
    public class AcceptFriendRequestDto
    {
        [Required]
        public string RequesterUsername { get; set; } = null!;
    }

}
