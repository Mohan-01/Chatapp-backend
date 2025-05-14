using ChatApp.UserService.Core.ResponseDTOs;
using Shared.EventContracts;

namespace ChatApp.UserService.Core.Interfaces
{
    public interface IUserEventsService
    {
        Task<ServiceResponse<string>> CreateUserProfileAsync(UserRegisteredEvent profile);
        Task<ServiceResponse<string>> ChangeUsernameAsync(UsernameChangedEvent usernameChangedEvent);
        Task<ServiceResponse<string>> ChangeEmailAsync(EmailChangedEvent emailChangedEvent);
        Task<ServiceResponse<string>> DeactivateUserAsync(UserDeletedEvent userDeletedEvent);
    }
}
