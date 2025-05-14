namespace ChatApp.ChatService.Core.Interfaces
{
    public interface IChatApiClient
    {
        Task<string> GetChatByUsernamesAsync(string sender, string receiver);
    }
}
