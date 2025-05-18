namespace ChatApp.ChatService.Core.Interfaces.NotUsing
{
    public interface IUserApiClient
    {
        Task<string> GetUsersByUsernamesBatch(string usernames);

        Task<string> SearchUsersByUsername(string username);
    }
}
