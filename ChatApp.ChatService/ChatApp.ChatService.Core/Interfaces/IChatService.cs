using MongoDB.Bson;
using ChatApp.ChatService.Core.Enums.Chat;
using ChatApp.ChatService.Core.Entities.Message;
using ChatApp.ChatService.Core.RequestResponseModels.Chat;
using ChatApp.ChatService.Core.DTOs.Chat;

namespace ChatApp.ChatService.Core.Interfaces
{
    public interface IChatService
    {
        Task<ServiceResponse<PrivateChatDto>> CreateOneToOneChatAsync(string username1, string username2);

        Task<ServiceResponse<PrivateChatDto>> GetChatByIdAsync(string chatId);
        Task<ServiceResponse<PrivateChatDto>> GetChatByUsernamesAsync(string username1, string username2);
        Task<ServiceResponse<List<PrivateChatDto>>> GetChatsByUsernameAsync(string username, ChatStatus chatStatus = ChatStatus.Active);
        Task<ServiceResponse<PrivateChatDto>> UpdateChatStatusAsync(string chatId, ChatStatus chatStatus);
        Task UpdateChatMessagesAsync(ObjectId chatId, Message messages);
        Task ArchiveChatAsync(ObjectId chatId);
    }
}
