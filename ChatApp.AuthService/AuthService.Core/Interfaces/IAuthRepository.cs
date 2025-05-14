using AuthService.Core.Models;
using Shared.EventContracts;

namespace AuthService.Core.Interfaces
{
    public interface IAuthRepository
    {
        Task<AuthUser?> GetByUsernameAsync(string username);
        Task<AuthUser?> GetByEmailAsync(string email);
        Task<int> GetTokenVersionWithUsername(string username);
        Task<bool> UsernameExistsAsync(string username);
        Task CreateUserAsync(AuthUser user);
        Task<bool> UpdatePasswordAsync(string username, string newPasswordHash);
        Task<bool> UpdateUsernameAsync(string currentUsername, string newUsername);
        Task<bool> UpdateEmailAsync(EmailChangedEvent emailChangedEvent);
        Task<bool> UpdateIsActiveStatusAsync(UserDeletedEvent userDeletedEvent);
    }
}
