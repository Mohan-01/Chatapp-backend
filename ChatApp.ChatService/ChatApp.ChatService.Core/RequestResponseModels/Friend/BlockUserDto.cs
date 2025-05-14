using System.ComponentModel.DataAnnotations;

namespace ChatApp.ChatService.Core.RequestResponseModels.Friend
{
    public class BlockUserDto
    {
        [Required]
        public string BlockedUsername { get; set; } = null!;
    }
}
