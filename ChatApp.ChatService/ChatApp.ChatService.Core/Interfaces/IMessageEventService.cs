using ChatApp.ChatService.Core.DTOs.Chat;
using ChatApp.ChatService.Core.RequestResponseModels.Chat;
using Shared.EventContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Core.Interfaces
{
    public interface IMessageEventService
    {
        Task<ServiceResponse<PrivateChatDto>> MessageRecievedEventAsync(MessageSentEvent messageSentEvent);
    }
}
