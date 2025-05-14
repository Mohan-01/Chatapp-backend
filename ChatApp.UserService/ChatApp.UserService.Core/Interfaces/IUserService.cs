using Shared.EventContracts;
using ChatApp.UserService.Core.Entities;
using ChatApp.UserService.Core.ResponseDTOs;
using Shared.Models.User;
using ChatApp.UserService.Core.RequestDTOs;

namespace ChatApp.UserService.Core.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResponse<UserDto>> GetByUsernameAsync(string username);
        Task<ServiceResponse<UserDto>> UpdateUserAsync(string username, UpdateUserRequest updateUserRequest);
        
        //Task<ServiceResponse<string>> DeleteUserAsync(string username);
        //Task<ServiceResponse<string>> CreateUserAsync(User2 user);
        //Task<ServiceResponse<UserDto>> GetByEmailAsync(string email);

        Task<ServiceResponse<List<UserDto>>> SearchUsersAsync(SearchUsersRequest dto);
        Task<ServiceResponse<List<UserDto>>> GetUsersBatchAsync(BatchUserRequest dto);

    }
}
