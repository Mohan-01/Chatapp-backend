using ChatApp.ChatService.Core.DTOs.Chat;
using ChatApp.ChatService.Core.Interfaces;
using ChatApp.ChatService.Core.Mappings;
using ChatApp.ChatService.Core.RequestResponseModels.Chat;
using ChatService.Entities.Chat;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.EventContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Core.Services
{
    public class MessageEventService: IMessageEventService
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<IMessageEventService> _logger;

        public MessageEventService(IChatRepository chatRepository, ILogger<IMessageEventService> logger)
        {
            _chatRepository = chatRepository;
            _logger = logger;
        }

        public async Task<ServiceResponse<PrivateChatDto>> MessageRecievedEventAsync(MessageSentEvent messageSentEvent)
        {
            try
            {
                _logger.LogInformation("Handling MessageSentEvent for ChatId: {ChatId}, MessageId: {MessageId}",
                    messageSentEvent.ChatId, messageSentEvent.MessageId);

                Chat updatedChat = await _chatRepository.MessageRecievedEventAsync(messageSentEvent);

                PrivateChatDto chatDto = MappingToDtos.MapPrivateChatToDto(updatedChat);

                // Optionally log or do something with updatedChat
                _logger.LogInformation("Chat updated successfully. ChatId: {ChatId}, TotalMessages: {Count}",
                    updatedChat.ChatId.ToString(), updatedChat.MessageIds.Count);

                return new ServiceResponse<PrivateChatDto>
                {
                    Success = true,
                    Message = "Message sent successfully",
                    Data = chatDto
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while handling MessageSentEvent. ChatId: {ChatId}, MessageId: {MessageId}",
                    messageSentEvent.ChatId, messageSentEvent.MessageId);

                // You could rethrow or swallow based on context — 
                // For example, if this is a background consumer, swallowing may be okay.
                return new ServiceResponse<PrivateChatDto> { Success = false, Message = ex.Message };
            }
        }

    }
}
