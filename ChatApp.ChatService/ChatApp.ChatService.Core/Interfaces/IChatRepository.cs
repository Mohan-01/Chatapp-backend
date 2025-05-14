using ChatApp.ChatService.Core.Entities.Message;
using ChatApp.ChatService.Core.Enums.Chat;
using ChatService.Entities.Chat;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ChatApp.ChatService.Core.Interfaces
{
    public interface IChatRepository
    {
        Task<Chat> CreateOneToOneChatAsync(IClientSessionHandle session, Chat chat);
        
        Task<Chat?> GetChatByIdAsync(string chatId);
        
        Task<Chat?> GetChatByUsernamesAsync(string username1, string username2);
        
        Task<List<Chat>?> GetChatsByUsernameAsync(string username, ChatStatus chatType = ChatStatus.Active);

        Task<Chat?> UpdateChatStatusAsync(string chatId, ChatStatus chatStatus);

        Task UpdateChatMessagesAsync(ObjectId chatId, Message messages);
        
        Task ArchiveChatAsync(ObjectId chatId);
    }
}
