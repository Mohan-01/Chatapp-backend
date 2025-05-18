namespace ChatApp.ChatService.Core.Interfaces.NotUsing
{
    public interface IChatApiClient
    {
        Task<string> GetChatByUsernamesAsync(string sender, string receiver);
    }
}
