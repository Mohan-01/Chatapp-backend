using AutoMapper;
using ChatApp.UserService.Core.Entities;
using Shared.Models.User;

namespace ChatApp.UserService.Core.Mappings
{
    public class MappingProfile: Profile
    {
        public MappingProfile() {
            CreateMap<User2, UserDto>();
        }
    }
}
