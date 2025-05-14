using ChatApp.ChatService.Core.Entities.Message;
using ChatApp.ChatService.Core.Enums.Message;

namespace ChatApp.ChatService.Core.Interfaces
{
    public interface IMessageRepository
    {
        // Get a specific message by its ID
        Task<Message> GetByIdAsync(string messageId);

        // Get all messages for a specific one-to-one chat
        Task<IEnumerable<Message>> GetMessagesByChatIdAsync(string chatId);

        // Get all messages sent or received by a specific user
        Task<IEnumerable<Message>> GetMessagesByUserIdAsync(string userId);

        // Get all unread messages for a specific user
        Task<IEnumerable<Message>> GetUnreadMessagesByUserIdAsync(string userId);

        // Create and send a new message
        Task<Message> SendMessageAsync(Message message);

        // Update a message (e.g., editing the text or marking it as read)
        Task UpdateMessageAsync(Message message);

        // Soft delete a message (mark as deleted)
        Task DeleteMessageAsync(string messageId);

        // Mark a specific message as read
        Task MarkMessageAsReadAsync(string messageId);

        // Mark all messages in a specific chat as read
        Task MarkChatMessagesAsReadAsync(string chatId, string userId);

        // Update the status of a message (e.g., Sent, Delivered, Seen)
        Task UpdateMessageStatusAsync(string messageId, MessageStatus status);
    }
}
