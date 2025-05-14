namespace ChatApp.ChatService.Core.DTOs.Message
{
    public class AttachmentDto
    {
        required public string AttachmentId { get; set; }
        required public string Url { get; set; }
        //public string? FileType { get; set; }
        //public long? FileSize { get; set; }
    }
}
