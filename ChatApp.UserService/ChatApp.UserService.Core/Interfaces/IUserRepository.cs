using ChatApp.UserService.Core.Entities;
using Shared.EventContracts;
using Shared.Models.User;

namespace ChatApp.UserService.Core.Interfaces
{
    public interface IUserRepository
    {

        // Get user by username
        Task<User2> GetByUsernameAsync(string username);

        // Get user by Email
        Task<User2> GetUserByEmailAsync(string email);

        Task<User2?> GetUserByResetTokenAsync(string resetToken);

        Task<List<User2>> SearchUsersAsync(string searchTerm);

        // Create a new user
        Task<bool> CreateUserAsync(User2 user);

        // Update an existing user
        Task<User2> UpdateUserAsync(UserDto user);

        // Delete a user by username
        Task DeleteUserAsync(string username);

        Task<bool> CreateUserProfileAsync(UserRegisteredEvent userEvent);
        Task<bool> ChangeUsernameAsync(UsernameChangedEvent usernameChangedEvent);
        Task<bool> UpdateEmailAsync(EmailChangedEvent emailChangedEvent);
        Task<bool> DeactivateUserAsync(UserDeletedEvent userDeletedEvent);

        Task<List<User2>> GetUsersBatchAsync(List<string> usernames);
    }
}