using ChatApp.ChatService.Core.DTOs;
using ChatApp.ChatService.Core.Entities.Friendship;
using ChatApp.ChatService.Core.Enums.Friend;
using ChatApp.ChatService.Core.Interfaces;
using ChatApp.ChatService.Core.RequestResponseModels.Chat;
using ChatApp.ChatService.Core.RequestResponseModels.Friend;
using Microsoft.Extensions.Logging;
using Shared.Models.User;

// No need of newtonsoft

namespace ChatApp.ChatService.Core.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly ILogger<IFriendshipService> _logger;
        private readonly IUserApiClient _userApiClient;

        public FriendshipService(IFriendshipRepository friendshipRepository, ILogger<IFriendshipService> logger, IUserApiClient userApiClient)
        {
            _friendshipRepository = friendshipRepository;
            _logger = logger;
            _userApiClient = userApiClient;
        }

        public async Task<ServiceResponse<string>> SendFriendRequestAsync(string username, SendFriendRequestDto dto)
        {
            try
            {
                if (username == dto.RecipientUsername)
                {
                    _logger.LogWarning("User '{Username}' tried to send a friend request to themselves.", username);
                    return new ServiceResponse<string>
                    {
                        Success = false,
                        Message = "You cannot send a friend request to yourself."
                    };
                }

                var userFetchResult = await FetchUserByUsernameAsync(dto.RecipientUsername);
                if (!userFetchResult.Success)
                    return new ServiceResponse<string> { 
                        Success = false, 
                        Message = userFetchResult.Message 
                    };

                var existingFriendship = await GetExistingFriendshipAsync(username, dto.RecipientUsername);

                if (existingFriendship != null)
                {
                    _logger.LogWarning("Friendship already exists between '{Username}' and '{RecipientUsername}'", username, dto.RecipientUsername);
                    return new ServiceResponse<string>{ Success = false, Message = "Friend request already exists or you are already friends." };
                }

                var friendship = new Friendship
                {
                    SenderUsername = username,
                    RecipientUsername = dto.RecipientUsername,
                    Status = FriendRequestStatus.Pending,
                    SentAt = DateTime.UtcNow
                };

                await _friendshipRepository.CreateAsync(friendship);

                _logger.LogInformation("Friend request from '{Username}' to '{RecipientUsername}' created successfully.", username, dto.RecipientUsername);
                return new ServiceResponse<string>{Success = true, Message = "Friend request sent." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while '{Username}' was sending a friend request to '{RecipientUsername}'", username, dto.RecipientUsername);
                return new ServiceResponse<string>{ Success = false, Message = "An error occurred while sending the friend request." };
            }
        }

        public async Task<ServiceResponse<string>> AcceptFriendRequestAsync(string username, AcceptFriendRequestDto dto)
        {
            try
            {
                var friendship = await GetExistingFriendshipAsync(username, dto.RequesterUsername);


                if (friendship == null)
                {
                    _logger.LogWarning("No friendship record found between '{Username}' and '{RequesterUsername}'", username, dto.RequesterUsername);
                    return new ServiceResponse<string>{ Success = false, Message = "Friend request not found or invalid." };
                }

                if (friendship.RecipientUsername != username || friendship.Status != FriendRequestStatus.Pending)
                {
                    _logger.LogWarning("Invalid friend request acceptance attempt by '{Username}' for request from '{RequesterUsername}'. Current Status: {Status}", username, dto.RequesterUsername, friendship.Status);
                    return new ServiceResponse<string>{ Success = false, Message = "Friend request not found or invalid." };
                }

                friendship.Status = FriendRequestStatus.Accepted;
                friendship.RespondedAt = DateTime.UtcNow;

                await _friendshipRepository.UpdateAsync(friendship);

                _logger.LogInformation("Friend request accepted. '{Username}' accepted friend request from '{RequesterUsername}'", username, dto.RequesterUsername);

                return new ServiceResponse<string>{ Success = true, Message = "Friend request accepted." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while accepting friend request for '{Username}' from '{RequesterUsername}'", username, dto.RequesterUsername);
                return new ServiceResponse<string>{ Success = false, Message = "An error occurred while accepting the friend request." };
            }
        }

        public async Task<ServiceResponse<string>> BlockUserAsync(string username, BlockUserDto dto)
        {
            try
            {
                var userFetchResult = await FetchUserByUsernameAsync(dto.BlockedUsername);
                if (!userFetchResult.Success)
                    return new ServiceResponse<string>{ Success = false, Message = userFetchResult.Message };

                var existingFriendship = await GetExistingFriendshipAsync(username, dto.BlockedUsername);

                if (existingFriendship != null)
                {
                    await _friendshipRepository.DeleteAsync(existingFriendship.Id.ToString());
                    _logger.LogInformation("Existing friendship between '{Username}' and '{BlockedUsername}' deleted before blocking.", username, dto.BlockedUsername);
                }

                var blockedFriendship = new Friendship
                {
                    SenderUsername = username,
                    RecipientUsername = dto.BlockedUsername,
                    Status = FriendRequestStatus.Blocked,
                    SentAt = DateTime.UtcNow
                };

                await _friendshipRepository.CreateAsync(blockedFriendship);

                _logger.LogInformation("User '{Username}' has successfully blocked '{BlockedUsername}'", username, dto.BlockedUsername);

                return new ServiceResponse<string>{ Success = true, Message = "User has been blocked." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while '{Username}' was blocking '{BlockedUsername}'", username, dto.BlockedUsername);
                return new ServiceResponse<string>{ Success = false, Message = "An error occurred while blocking the user." };
            }
        }

        public async Task<ServiceResponse<string>> UnfriendUserAsync(string username, UnfriendUserDto dto)
        {
            try
            {
                var userFetchResult = await FetchUserByUsernameAsync(dto.FriendUsername);
                if (!userFetchResult.Success)
                {
                    _logger.LogWarning("User '{FriendUsername}' not found while '{Username}' attempted to unfriend.", dto.FriendUsername, username);
                    return new ServiceResponse<string>{ Success = false, Message = userFetchResult.Message };
                }

                var friendship = await GetExistingFriendshipAsync(username, dto.FriendUsername);

                if (friendship == null || friendship.Status != FriendRequestStatus.Accepted)
                {
                    _logger.LogWarning("Unfriend attempt failed: '{Username}' is not friends with '{FriendUsername}'.", username, dto.FriendUsername);
                    return new ServiceResponse<string>{ Success = false, Message = "You are not friends with this user." };
                }

                await _friendshipRepository.DeleteAsync(friendship.Id.ToString());

                _logger.LogInformation("User '{Username}' unfriended '{FriendUsername}' successfully.", username, dto.FriendUsername);

                return new ServiceResponse<string>{ Success = true, Message = "User has been unfriended." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while '{Username}' was trying to unfriend '{FriendUsername}'.", username, dto.FriendUsername);
                return new ServiceResponse<string>{ Success = false, Message = "An error occurred while unfriending the user." };
            }
        }

        public async Task<ServiceResponse<List<FriendDto>>> GetFriendsListAsync(string username)
        {
            try
            {
                _logger.LogInformation("Fetching accepted friendships for user '{Username}'.", username);
                var friendships = await _friendshipRepository.GetAcceptedFriendshipsAsync(username);

                if (friendships == null || friendships.Count == 0)
                {
                    _logger.LogInformation("No friends found for user '{Username}'.", username);
                    return new ServiceResponse<List<FriendDto>>{ Success = true, Message = "No friends found.", Data = [] };
                }

                var friends = new List<FriendDto>();

                foreach (var friendship in friendships)
                {
                    var friendUsername = friendship.SenderUsername == username ? friendship.RecipientUsername : friendship.SenderUsername;

                    ServiceResponse<List<UserDto>> fetchResult = await FetchUserByUsernameAsync(friendUsername);

                    if (fetchResult.Success && fetchResult.Data != null && fetchResult.Data.Count != 0)
                    {
                        UserDto? user = fetchResult.Data.FirstOrDefault();
                        if (user != null)
                        {
                            FriendDto friend = MapUserDtoToFriendDto(user, friendship.Status);
                            friends.Add(friend);
                        }
                        else
                        {
                            _logger.LogWarning("No user data found after deserialization for '{FriendUsername}'.", friendUsername);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Could not fetch user '{FriendUsername}': {Message}", friendUsername, fetchResult.Message);
                    }
                }


                _logger.LogInformation("Successfully fetched friends list for user '{Username}'.", username);
                return new ServiceResponse<List<FriendDto>>{ Success = true, Message = "Friends list retrieved.", Data = friends };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the friends list for user '{Username}'.", username);
                return new ServiceResponse<List<FriendDto>>{ Success = false, Message = "Failed to retrieve friends list.", Data = [] };
            }
        }

        public async Task<ServiceResponse<List<FriendDto>>> SearchUsersAsync(string username, string searchTerm)
        {
            try
            {
                _logger.LogInformation("Searching users with term '{SearchTerm}'.", searchTerm);

                ServiceResponse<List<UserDto>> response = await SearchUserByUsernameAsync(searchTerm);

                List<UserDto>? users = response.Data;

                if (users == null || users.Count == 0)
                {
                    _logger.LogWarning("No users found for search term '{SearchTerm}'.", searchTerm);
                    return new ServiceResponse<List<FriendDto>>{
                        Success=true, 
                        Message = "No users found.", 
                        Data = []
                      };
                }

                var friends = new List<FriendDto>();

                foreach (var user in users)
                {
                    if (user.Username == username)
                        continue; // Don't include yourself

                    // Fetch the friendship between the searcher and this user
                    var friendship = await _friendshipRepository.GetFriendshipAsync(username, user.Username);

                    string friendshipStatus;

                    if (friendship == null)
                    {
                        friendshipStatus = "None"; // No request sent or received
                    }
                    else
                    {
                        friendshipStatus = friendship.Status.ToString(); // Pending / Accepted / Blocked
                    }

                    var friendDto = new FriendDto
                    {
                        Username = user.Username,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        ProfilePicture = user.ProfilePicture,
                        UserStatus = user.Status,
                        FriendshipStatus = friendshipStatus,
                        LastSeen = user.LastSeen
                    };

                    friends.Add(friendDto);
                }

                _logger.LogInformation("Successfully retrieved {UserCount} users for search term '{SearchTerm}'.", users.Count, searchTerm);
                return new ServiceResponse<List<FriendDto>>{ Success = true, Message = "Search list retrieved.", Data = friends };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching users with term '{SearchTerm}'.", searchTerm);
                return new ServiceResponse<List<FriendDto>>{ Success = false, Message = "An error occurred while searching users." };
            }
        }

        public async Task<ServiceResponse<List<FriendRequestDto>>> GetFriendRequestsAsync(string username)
        {
            try
            {
                _logger.LogInformation("Fetching pending friend requests for user '{Username}'.", username);

                var friendRequests = await _friendshipRepository.GetPendingRequestsAsync(username);

                if (friendRequests == null || friendRequests.Count == 0)
                {
                    _logger.LogInformation("No pending friend requests found for user '{Username}'.", username);
                    return new ServiceResponse<List<FriendRequestDto>>{ Success = true, Message = "No friend requests found." };
                }

                var requestDtos = new List<FriendRequestDto>();

                foreach (var friendRequest in friendRequests)
                {

                    ServiceResponse<List<UserDto>> response = await FetchUserByUsernameAsync(friendRequest.SenderUsername);

                    List<UserDto> requesterList = response.Data;
                    var requester = requesterList?.FirstOrDefault();

                    if (requester == null)
                    {
                        _logger.LogWarning("Deserialization failed or returned null for sender '{SenderUsername}'.", friendRequest.SenderUsername);
                        continue;
                    }

                    requestDtos.Add(new FriendRequestDto
                    {
                        SenderUsername = friendRequest.SenderUsername,
                        SentAt = friendRequest.SentAt,
                        SenderProfilePicture = requester.ProfilePicture,
                        SenderFirstName = requester.FirstName,
                        SenderLastName = requester.LastName
                    });
                }

                _logger.LogInformation("Successfully retrieved {RequestCount} friend requests for user '{Username}'.", requestDtos.Count, username);

                return new ServiceResponse<List<FriendRequestDto>>{ Success = true, Message = "Friend requests retrieved successfully.", Data = requestDtos };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching friend requests for user '{Username}'.", username);
                return new ServiceResponse<List<FriendRequestDto>>{ Success = false, Message = "An error occurred while retrieving friend requests." };
            }
        }

        private async Task<ServiceResponse<List<UserDto>>> FetchUserByUsernameAsync(string username)
        {
            try
            {
                var content = await _userApiClient.GetUsersByUsernamesBatch(username);

                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("User '{Username}' not found during fetch operation.", username);
                    throw new Exception("User not found.");
                }

                ServiceResponse<List<UserDto>> response = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResponse<List<UserDto>>>(content);

                if (response == null || !response.Success || response.Data == null || response.Data.Count == 0)
                {
                    _logger.LogWarning($"Deserialization failed or returned unsuccessful response for username: {username}");
                    throw new Exception("Failed to retrieve users.");
                }

                List<UserDto> users = response.Data;

                return new ServiceResponse<List<UserDto>>{ Success = true, Message = "User found", Data = users };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user '{Username}'", username);
                throw;
            }
        }

        private async Task<ServiceResponse<List<UserDto>>> SearchUserByUsernameAsync(string searchTerm)
        {
            try
            {
                var content = await _userApiClient.SearchUsersByUsername(searchTerm);
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("No users found for search term '{SearchTerm}'.", searchTerm);
                    // throw error
                    throw new Exception("No users found.");
                }
                ServiceResponse<List<UserDto>> response = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResponse<List<UserDto>>>(content);
                if (response == null || !response.Success || response.Data == null || response.Data.Count == 0)
                {
                    _logger.LogWarning("Deserialization failed or returned unsuccessful response for search term '{SearchTerm}'.", searchTerm);
                    throw new Exception("Failed to retrieve users.");
                }
                List<UserDto> users = response.Data;
                return new ServiceResponse<List<UserDto>>{ Success = true, Message = "Users found", Data = users };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with term '{SearchTerm}'", searchTerm);

                // throw error
                throw;
            }
        }

        private async Task<Friendship?> GetExistingFriendshipAsync(string username1, string username2)
        {
            try
            {
                return await _friendshipRepository.GetFriendshipAsync(username1, username2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existing friendship between '{Username1}' and '{Username2}'", username1, username2);
                throw; // Let caller handle if needed
            }
        }

        private static FriendDto MapUserDtoToFriendDto(UserDto user, FriendRequestStatus friendRequestStatus)
        {
            return new FriendDto
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePicture = user.ProfilePicture,
                UserStatus = user.Status,
                FriendshipStatus = friendRequestStatus.ToString(),
                LastSeen = user.LastSeen
            };
        }
    }
}
