namespace ChatApp.ChatService.Core.Enums.Message
{
    public enum MessageStatus
    {
        Sent,       // Message sent by the sender but not yet delivered
        Delivered,  // Message delivered to the recipient's device
        Seen,       // Message seen/read by the recipient
        Failed,     // Message failed to send/deliver
        Pending,    // Message waiting to be sent or processed
        Acknowledged, // Message acknowledged by the server/recipient
        Deleted,    // Message deleted by sender/recipient
        Archived,   // Message archived and out of active view
        Expired     // Message has expired and is no longer valid
    }

}
