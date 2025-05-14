using ChatApp.ChatService.Core.Entities.Friendship;
using ChatApp.ChatService.Core.Enums.Friend;

namespace ChatApp.ChatService.Core.Interfaces
{
    public interface IFriendshipRepository
    {
        Task<Friendship?> GetFriendshipAsync(string userId1, string userId2);
        Task<List<Friendship>> GetAcceptedFriendshipsAsync(string userId);
        Task<List<Friendship>> GetPendingRequestsAsync(string userId);
        Task CreateAsync(Friendship friendship);
        Task UpdateAsync(Friendship friendship);
        Task DeleteAsync(string friendshipId);
        Task<List<Friendship>> GetFriendRequestsAsync(string userId, FriendRequestStatus status);

    }
}
