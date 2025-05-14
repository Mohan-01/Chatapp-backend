using ChatApp.ChatService.Core.Enums.Message;

namespace ChatApp.ChatService.Core.DTOs.Message
{
    public class MessageDto
    {
        required public string MessageId { get; set; }
        required public string From { get; set; }
        required public string To { get; set; }
        required public DateTime Time { get; set; }
        public string? Text { get; set; }
        required public MessageType MessageType { get; set; } 
        public List<AttachmentDto> Attachments { get; set; } = [];
        public string? RepliedTo { get; set; }
        required public bool IsEdited { get; set; }
        required public string MessageStatus { get; set; } = string.Empty;
    }
}
