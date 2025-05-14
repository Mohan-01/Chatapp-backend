namespace ChatApp.ChatService.Core.Interfaces
{
    public interface IUserApiClient
    {
        Task<string> GetUsersByUsernamesBatch(string usernames);

        Task<string> SearchUsersByUsername(string username);
    }
}
