namespace Shared.HttpClients.Interfaces
{
    public interface IChatApiClient
    {
        Task<string> GetChatByUsernamesAsync(string sender, string receiver);
    }
}
