using ChatApp.UserService.Core.Entities;
using Shared.Models.User;

namespace UserService.Mappings
{
    public static class MappingToDtos
    {
        public static UserDto MapUserToDto(User2 user)
        {
            return new UserDto
            {
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                ProfilePicture = user.ProfilePicture,
                Status = user.Status.ToString(),
                LastSeen = user.LastSeen
            };
        }
    }
}
