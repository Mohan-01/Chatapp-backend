using MongoDB.Bson;
using MongoDB.Driver;
using ChatService.Entities.Chat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ChatApp.ChatService.Core.Interfaces;
using ChatApp.ChatService.Core.Enums.Chat;
using ChatApp.ChatService.Core.Entities.Message;

namespace ChatApp.ChatService.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly IMongoCollection<Chat> _chats;
        private readonly ILogger<IChatRepository> _logger;

        public ChatRepository(
            IConfiguration config, 
            IMongoClient client, 
            ILogger<IChatRepository> logger)
        {
            var databaseName = config["MongoDbSettings:DatabaseName"];
            var collectionName = "Chats";

            var database = client.GetDatabase(databaseName);
            _chats = database.GetCollection<Chat>(collectionName);
            _logger = logger;
        }
        
        // DONE
        public async Task<Chat> CreateOneToOneChatAsync(IClientSessionHandle session, Chat chat)
        {
            _logger.LogInformation("Attempting to create one-to-one chat between {Username1} and {Username2}.", chat.Participants[0], chat.Participants[1]);

            try
            {
                await _chats.InsertOneAsync(session, chat);
                _logger.LogInformation("New chat created with ID {ChatId}.", chat.ChatId);

                return chat; // 👉 Return the chat you just inserted (not newChat, which didn't exist)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a one-to-one chat between {Username1} and {Username2}.", chat.Participants[0], chat.Participants[1]);
                throw new ApplicationException("Failed to create chat. See inner exception for details.", ex);
            }
        }

        // DONE
        public async Task<Chat?> GetChatByIdAsync(string chatId)
        {
            _logger.LogInformation("Fetching chat details from the database for chatId: {ChatId}", chatId);

            try
            {
                // Filter to find the chat by chatId
                FilterDefinition<Chat> filter = Builders<Chat>.Filter.Eq(c => c.ChatId, ObjectId.Parse(chatId));

                // Find the chat
                var chat = await _chats.Find(filter).FirstOrDefaultAsync();

                if (chat == null)
                {
                    _logger.LogWarning("No chat found with chatId: {ChatId}", chatId);
                    return null;
                }

                // Return the chat entity
                return chat;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching chat by chatId: {ChatId} in repository.", chatId);
                throw new ApplicationException("An error occurred while fetching the chat details.", ex);
            }
        }


        // DONE
        public async Task<Chat?> GetChatByUsernamesAsync(string username1, string username2)
        {
            _logger.LogInformation("Fetching chat document between {Username1} and {Username2}", username1, username2);

            var chat = await _chats
                .Find(c => c.Participants.Contains(username1) && c.Participants.Contains(username2) && c.ChatType == ChatType.Private)
                .FirstOrDefaultAsync();

            if (chat == null)
            {
                _logger.LogInformation("No chat document found between {Username1} and {Username2}.", username1, username2);
            }

            return chat; // 👉 Return NULL if not found, no exception
        }

        // DONE
        public async Task<List<Chat>?> GetChatsByUsernameAsync(string username, ChatStatus chatStatus = ChatStatus.Active)
        {
            _logger.LogInformation("Fetching all {ChatStatus} chats for user {Username}.", chatStatus, username);

            try
            {
                var chats = await _chats
                    .Find(c => c.Participants.Contains(username) && c.ChatStatus == chatStatus)
                    .ToListAsync();

                if (chats == null || chats.Count == 0)
                {
                    _logger.LogWarning("No {ChatStatus} chats found for user {Username}.", chatStatus, username);
                }

                return chats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching chats for user {Username} with status {ChatStatus}.", username, chatStatus);
                throw new ApplicationException("An error occurred while fetching chats.", ex);
            }
        }

        // DONE
        public async Task<Chat?> UpdateChatStatusAsync(string chatId, ChatStatus chatStatus)
        {
            _logger.LogInformation("Attempting to update chat status for chatId: {ChatId} to {ChatStatus}", chatId, chatStatus);

            try
            {
                FilterDefinition<Chat> filter = Builders<Chat>.Filter.Eq(c => c.ChatId, ObjectId.Parse(chatId));
                UpdateDefinition<Chat> update = Builders<Chat>.Update.Set(c => c.ChatStatus, chatStatus);

                UpdateResult result = await _chats.UpdateOneAsync(filter, update);

                if (result.ModifiedCount == 0)
                {
                    string message = $"No chat found with chatId: {chatId} or it may already have the status {chatStatus}.";
                    _logger.LogWarning(message);
                    return null; // No chat updated, so return null
                }

                // Fetch and return the updated chat
                return await _chats.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating chat status for chatId: {ChatId}.", chatId);
                throw new InvalidOperationException("An error occurred while updating the chat status.", ex);
            }
        }


        public async Task UpdateChatMessagesAsync(ObjectId chatId, Message newMessage)
        {
            if (newMessage == null)
                throw new ArgumentNullException(nameof(newMessage), "New message cannot be null.");

            _logger.LogInformation("Adding new message to chat {ChatId}", chatId);

            FilterDefinition<Chat> filter = Builders<Chat>.Filter.Eq(c => c.ChatId, chatId);
            var update = Builders<Chat>.Update.Push(c => c.Messages, newMessage);

            var result = await _chats.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                var message = $"Failed to add message. Chat {chatId} may not exist.";
                _logger.LogWarning(message);
                throw new InvalidOperationException(message);
            }
        }

        public async Task ArchiveChatAsync(ObjectId chatId)
        {
            _logger.LogInformation("Archiving chat {ChatId}", chatId);

            FilterDefinition<Chat> filter = Builders<Chat>.Filter.Eq(c => c.ChatId, chatId);
            var update = Builders<Chat>.Update.Set(c => c.ChatStatus, ChatStatus.Archive);

            var result = await _chats.UpdateOneAsync(filter, update);

            if (result.ModifiedCount == 0)
            {
                var message = $"Failed to archive chat. Chat {chatId} may not exist or is already archived.";
                _logger.LogWarning(message);
                throw new InvalidOperationException(message);
            }
        }
    }
}
