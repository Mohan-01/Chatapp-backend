namespace Shared.EventContracts
{
    public class MessageSentEvent
    {
        public string ChatId { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
    }
}
