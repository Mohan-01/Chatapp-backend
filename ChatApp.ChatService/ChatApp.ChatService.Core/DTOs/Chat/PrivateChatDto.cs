using ChatApp.ChatService.Core.DTOs.Message;
using ChatApp.ChatService.Core.Enums.Chat;

namespace ChatApp.ChatService.Core.DTOs.Chat
{
    public class PrivateChatDto
    {
        required public string ChatId { get; set; }
        required public string Username1 { get; set; }
        required public string Username2 { get; set; }
        public List<MessageDto> Messages { get; set; } = [];  // Messages in chat
        required public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageTime { get; set; }
        required public ChatStatus ChatStatus { get; set; }
    }
}
