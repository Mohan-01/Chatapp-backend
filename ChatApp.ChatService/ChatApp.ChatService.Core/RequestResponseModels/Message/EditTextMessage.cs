namespace ChatApp.ChatService.Core.RequestResponseModels.Message
{
    public class EditTextMessage
    {
        required public string MessageId { get; set; } = null!;
        required public string Text { get; set; } = null!;
    }
}
