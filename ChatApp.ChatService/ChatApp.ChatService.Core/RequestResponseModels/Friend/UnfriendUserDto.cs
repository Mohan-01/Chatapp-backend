using System.ComponentModel.DataAnnotations;

namespace ChatApp.ChatService.Core.RequestResponseModels.Friend
{
    public class UnfriendUserDto
    {
        [Required]
        public string FriendUsername { get; set; } = null!;
    }
}
