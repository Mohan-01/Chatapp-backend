using AutoMapper;
using ChatApp.UserService.Core.Entities;
using Shared.Models.User;
namespace ChatApp.UserService.API.Mappings
{
    public class MappingProfile: Profile
    {
        public MappingProfile() {
            CreateMap<User2, UserDto>();
        }
    }
}
