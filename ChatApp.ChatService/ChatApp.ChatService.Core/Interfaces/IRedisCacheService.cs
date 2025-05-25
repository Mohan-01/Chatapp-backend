using ChatApp.ChatService.Core.DTOs.Message;

namespace ChatApp.ChatService.Core.Interfaces
{
    public interface IRedisCacheService
    {
        Task AddUnsentMessageAsync(string username, MessageDto message);
        Task<List<MessageDto>> GetUnsentMessagesAsync(string username);
        Task ClearUnsentMessagesAsync(string username);
    }
}
