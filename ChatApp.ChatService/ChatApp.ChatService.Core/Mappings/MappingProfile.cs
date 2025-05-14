using AutoMapper;
using ChatApp.ChatService.Core.DTOs.Chat;
using ChatService.Entities.Chat;

namespace ChatService.Mappings
{
    public class MappingProfile: Profile
    {
        public MappingProfile() {
            //CreateMap<Message, MessageDto>();
            CreateMap<Chat,  ChatDto>();
        }
    }
}
