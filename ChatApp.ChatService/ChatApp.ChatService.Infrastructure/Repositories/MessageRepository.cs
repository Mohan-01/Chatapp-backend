using ChatApp.ChatService.Core.DTOs.Chat;
using ChatApp.ChatService.Core.Entities.Message;
using ChatApp.ChatService.Core.Enums.Message;
using ChatApp.ChatService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatApp.ChatService.Infrastructure.Repositories
{
    public class MessageRepository: IMessageRepository
    {
        private readonly IMongoCollection<Message> _messages;
        private readonly ILogger<IMessageRepository> _logger;
        private readonly IChatApiClient _chatApiClient;
        private readonly string _internalApiSecret;

        public MessageRepository(IConfiguration config, IMongoClient client, ILogger<IMessageRepository> logger, IChatApiClient chatApiClient)
        {
            var databaseName = config["MongoDbSettings:DatabaseName"];
            var collectionName = "Messages";

            var database = client.GetDatabase(databaseName);
            _messages = database.GetCollection<Message>(collectionName);
            _logger = logger;

            _chatApiClient = chatApiClient;
            _internalApiSecret = config["InternalApi:Secret"] ?? "your-fallback-secret";
        }

        // Get a specific message by its ID
        public async Task<Message> GetByIdAsync(string messageId)
        {
            return await _messages.Find(m => m.MessageId == messageId).FirstOrDefaultAsync();
        }

        // Get all messages for a specific one-to-one chat
        public async Task<IEnumerable<Message>> GetMessagesByChatIdAsync(string chatId)
        {
            return await _messages
                .Find(m => m.ChatId == ObjectId.Parse(chatId))
                .SortBy(m => m.SentAt)
                .ToListAsync();
        }

        // Get all messages sent or received by a specific user
        public async Task<IEnumerable<Message>> GetMessagesByUserIdAsync(string userId)
        {
            return await _messages
                .Find(m => m.SenderUsername == userId || m.ReceiverUsername == userId)
                .SortBy(m => m.SentAt)
                .ToListAsync();
        }

        // Get all unread messages for a specific user
        public async Task<IEnumerable<Message>> GetUnreadMessagesByUserIdAsync(string userId)
        {
            return await _messages
                .Find(m => (m.ReceiverUsername == userId || m.SenderUsername == userId) && m.MessageStatus != MessageStatus.Seen)
                .SortBy(m => m.SentAt)
                .ToListAsync();
        }

        // Create and send a new message
        public async Task<Message> SendMessageAsync(Message message)
        {
            try
            {
                // Log the message to be inserted
                _logger.LogInformation("Inserting message {MessageId} into chat {ChatId}.", message.MessageId, message.ChatId);

                // Ensure the SentAt field is correctly set before insertion
                message.SentAt = DateTime.UtcNow;

                // Insert the message into the database
                await _messages.InsertOneAsync(message);

                // Log success message after insertion
                _logger.LogInformation("Message {MessageId} successfully sent in chat {ChatId}.", message.MessageId, message.ChatId);

                return message;  // Return the message upon success
            }
            catch (Exception ex)
            {
                // Log the error and throw with a clear message
                _logger.LogError(ex, "Failed to insert message {MessageId} into chat {ChatId}.", message.MessageId, message.ChatId);
                throw new Exception("Failed to send the message. Database operation failed.", ex);
            }
        }

        // Update a message (e.g., editing the text or marking it as read)
        public async Task UpdateMessageAsync(Message message)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.MessageId, message.MessageId);
            await _messages.ReplaceOneAsync(filter, message);
        }

        // Soft delete a message (mark as deleted)
        public async Task DeleteMessageAsync(string messageId)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.MessageId, messageId);
            var update = Builders<Message>.Update.Set(m => m.MessageStatus, MessageStatus.Deleted);
            await _messages.UpdateOneAsync(filter, update);
        }

        // Mark a specific message as read
        public async Task MarkMessageAsReadAsync(string messageId)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.MessageId, messageId);
            var update = Builders<Message>.Update.Set(m => m.MessageStatus, MessageStatus.Seen);
            await _messages.UpdateOneAsync(filter, update);
        }

        // Mark all messages in a specific chat as read
        public async Task MarkChatMessagesAsReadAsync(string chatId, string userId)
        {
            var chatObjectId = ObjectId.Parse(chatId);
            var filter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.ChatId, chatObjectId),
                Builders<Message>.Filter.Eq(m => m.ReceiverUsername, userId),
                Builders<Message>.Filter.Eq(m => m.MessageStatus, MessageStatus.Seen)
            );
            var update = Builders<Message>.Update.Set(m => m.MessageStatus, MessageStatus.Seen);
            await _messages.UpdateManyAsync(filter, update);
        }

        // Update the status of a message (e.g., sent, delivered, read)
        public async Task UpdateMessageStatusAsync(string messageId, MessageStatus status)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.MessageId, messageId);
            var update = Builders<Message>.Update.Set(m => m.MessageStatus, status);
            await _messages.UpdateOneAsync(filter, update);
        }
    }
}
