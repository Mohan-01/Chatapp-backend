using ChatApp.UserService.Core.Mappings;
using ChatApp.UserService.Core.Entities;
using ChatApp.UserService.Core.Interfaces;
using ChatApp.UserService.Core.ResponseDTOs;
using Microsoft.Extensions.Logging;
using Shared.Models.User;
using ChatApp.UserService.Core.RequestDTOs;
using ChatApp.UserService.Core.Extensions;

namespace ChatApp.UserService.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<IUserService> _logger;

        public UserService(IUserRepository userRepository, ILogger<IUserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }
        
        #region Get User
        public async Task<ServiceResponse<UserDto>> GetByUsernameAsync(string username)
        {
            User2 user = await _userRepository.GetByUsernameAsync(username);
            
            if (user == null)
            {
                return new ServiceResponse<UserDto>(false, "User not found", null);
            }

            return new ServiceResponse<UserDto>(true, "User found", MappingToDtos.MapUserToDto(user));
        }
        /*
        public async Task<ServiceResponse<UserDto>> GetByEmailAsync(string email)
        {
            User2 user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null)
            {
                return new ServiceResponse<UserDto>(false, "User not found", null);
            }

            return new ServiceResponse<UserDto>(true, "User found", MappingToDtos.MapUserToDto(user));
        }
        */
        
        #endregion
        
        /*
        public async Task<ServiceResponse<string>> CreateUserAsync(User2 user)
        {
            try
            {
                bool result = await _userRepository.CreateUserAsync(user);
                
                if (result)
                {
                    _logger.LogInformation("User {Username} created successfully", user.Username);
                    return new ServiceResponse<string>(true, "User created successfully", null);
                }
                else
                {
                    _logger.LogWarning("Unable to process request now try again some time later.");
                    return new ServiceResponse<string>(false, "Unable to process request now try again some time later.", null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}", user.Username);
                return new ServiceResponse<string>(false, $"User creation failed: {ex}", null);
            }
        }
        */
        
        public async Task<ServiceResponse<UserDto>> UpdateUserAsync(string username, UpdateUserRequest updateUserRequest)
        {
            ServiceResponse<UserDto> user = await GetByUsernameAsync(username);

            if (!user.Success || user.Data == null)
            {
                return new ServiceResponse<UserDto>(false, "User not found", null);
            }

            user.Data.FirstName = updateUserRequest.FirstName ?? user.Data.FirstName;
            user.Data.MiddleName = updateUserRequest.MiddleName ?? user.Data.MiddleName;
            user.Data.LastName = updateUserRequest.LastName ?? user.Data.LastName;
            user.Data.Phone = updateUserRequest.Phone ?? user.Data.Phone;
            user.Data.ProfilePicture = updateUserRequest.ProfilePicture ?? user.Data.ProfilePicture;
            user.Data.Status = updateUserRequest.Status ?? user.Data.Status;

            var updatedUser = await _userRepository.UpdateUserAsync(user.Data);

            return new ServiceResponse<UserDto>(true, "User updated successfully", MappingToDtos.MapUserToDto(updatedUser));
        }
        
        public async Task<ServiceResponse<List<UserDto>>> SearchUsersAsync(SearchUsersRequest dto)
        {
            List<User2>? users = await _userRepository.SearchUsersAsync(dto.SearchTerm);

            if (users == null || users.Count == 0)
            {
                _logger.LogInformation("No users found for search term: {SearchTerm}", dto.SearchTerm);
                return new ServiceResponse<List<UserDto>>(false, "No users found", null);
            }

            List<UserDto> resultedUsers = [..users.Select(user => MappingToDtos.MapUserToDto(user))];

            _logger.LogInformation("Returning {Count} users from service", resultedUsers.Count);

            return new ServiceResponse<List<UserDto>>(true, "Users found", resultedUsers);
        }

        public async Task<ServiceResponse<List<UserDto>>> GetUsersBatchAsync(BatchUserRequest dto)
        {
            if (dto == null || dto.Usernames == null)
            {
                _logger.LogInformation("No usernames provided for batch request");
                return new ServiceResponse<List<UserDto>>(false, "No usernames provided", null);
            }
            
            List<string> usernames = [.. dto.Usernames.Split(',')];
            if (usernames == null || usernames.Count == 0)
            {
                _logger.LogInformation("No usernames provided for batch request");
                return new ServiceResponse<List<UserDto>>(false, "No usernames provided", null);
            }

            _logger.LogInformation("Fetching users for batch request: {Usernames}", string.Join(", ", usernames));
            
            List<User2>? users = await _userRepository.GetUsersBatchAsync(usernames);
            if (users == null || users.Count == 0)
            {
                _logger.LogInformation("No users found for usernames: {Usernames}", string.Join(", ", usernames));
                return new ServiceResponse<List<UserDto>>(false, "No users found", null);
            }

            List<UserDto> resultedUsers = [.. users.Select(user => MappingToDtos.MapUserToDto(user))];
            if (resultedUsers == null || resultedUsers.Count == 0)
            {
                _logger.LogInformation("No users found for usernames: {Usernames}", string.Join(", ", usernames));
                return new ServiceResponse<List<UserDto>>(false, "No users found", null);
            }

            _logger.LogInformation("Returning {Count} users from service", resultedUsers.Count);
            return new ServiceResponse<List<UserDto>>(true, "Users found", resultedUsers);
        }
    }
}