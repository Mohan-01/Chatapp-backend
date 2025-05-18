namespace ChatApp.ChatService.Core.Interfaces.NotUsing
{
    public interface IMessageApiClient
    {
        Task<string> GetMessagesByChatId(string chatId);
    }
}
