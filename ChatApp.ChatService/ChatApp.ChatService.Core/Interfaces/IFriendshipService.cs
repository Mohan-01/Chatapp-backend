using ChatApp.ChatService.Core.DTOs;
using ChatApp.ChatService.Core.RequestResponseModels.Chat;
using ChatApp.ChatService.Core.RequestResponseModels.Friend;

namespace ChatApp.ChatService.Core.Interfaces
{
    public interface IFriendshipService
    {
        Task<ServiceResponse<string>> SendFriendRequestAsync(string requesterId, SendFriendRequestDto addresseeUsername);
        Task<ServiceResponse<string>> AcceptFriendRequestAsync(string username, AcceptFriendRequestDto dto);
        Task<ServiceResponse<string>> BlockUserAsync(string username, BlockUserDto targetUsername);
        Task<ServiceResponse<string>> UnfriendUserAsync(string username, UnfriendUserDto friendUsername);
        //Task<List<UserDto>> GetFriendsListAsync(string username);  // Returns a List<UserDto>
        //Task<List<UserDto>> SearchUsersAsync(string searchTerm);  // Accepts a search term 
        Task<ServiceResponse<List<FriendDto>>> GetFriendsListAsync(string username);
        Task<ServiceResponse<List<FriendDto>>> SearchUsersAsync(string username, string searchTerm);
        Task<ServiceResponse<List<FriendRequestDto>>> GetFriendRequestsAsync(string username);

    }
}
