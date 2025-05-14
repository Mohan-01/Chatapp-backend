using ChatApp.ChatService.Core.DTOs.Message;
using ChatApp.ChatService.Core.Enums.Chat;

namespace ChatApp.ChatService.Core.DTOs.Chat
{
    public class ChatDto
    {
        public string ChatId { get; set; } = string.Empty;  // Chat ID (can be ObjectId)
        public string User1Id { get; set; } = string.Empty;  // User 1 ID
        public string User2Id { get; set; } = string.Empty;  // User 2 ID
        public string Username1 { get; set; } = string.Empty;
        public string Username2 { get; set; } = string.Empty;
        public List<MessageDto> Messages { get; set; } = [];
        public DateTime CreatedAt { get; set; }  // Timestamp of chat creation
        public ChatStatus ChatStatus { get; set; }  // Chat status (e.g., "Active", "Archived")
    }
}
