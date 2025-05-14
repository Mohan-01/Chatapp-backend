using ChatApp.UserService.Core.Entities;
using Shared.Models.User;

namespace ChatApp.UserService.Core.Mappings
{
    public static class MappingToDtos
    {
        public static UserDto MapUserToDto(User2 user)
        {
            return new UserDto
            {
                Username = user.Username,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Roles = user.Roles,
                ProfilePicture = user.ProfilePicture,
                Status = user.Status.ToString(),
                LastSeen = user.LastSeen
            };
        }
    }
}
