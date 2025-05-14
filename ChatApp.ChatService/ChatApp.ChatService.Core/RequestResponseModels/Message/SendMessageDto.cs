namespace ChatApp.ChatService.Core.RequestResponseModels.Message
{
    public class SendMessageDto
    {
        required public string ChatId { get; set; } = null!;  // ID of the chat where the message is sent
        required public string From { get; set; } = null!;  // User ID of the sender (can be ObjectId)
        required public string To { get; set; } = null!;  // User ID or Group ID
        public DateTime Time { get; set; }  // Timestamp when the message was sent
        required public string Text { get; set; } = null!;  // Content of the message
        required public string MessageType { get; set; } = null!;  // Message type (Text, Image, Video, File)
        public string RepliedTo { get; set; } = string.Empty;  // ID of the original message being replied to (optional)
    }
}
