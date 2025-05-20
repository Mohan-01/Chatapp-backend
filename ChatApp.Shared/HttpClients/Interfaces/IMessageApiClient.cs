namespace Shared.HttpClients.Interfaces
{
    public interface IMessageApiClient
    {
        Task<string> GetMessagesByChatId(string chatId);
    }
}
