namespace ChatApp.ChatService.Core.Interfaces
{
    public interface IMessageApiClient
    {
        Task<string> GetMessagesByChatId(string chatId);
    }
}
