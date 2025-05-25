using ChatApp.ChatService.Core.DTOs.Message;
using ChatApp.ChatService.Core.Entities.Message;
using ChatApp.ChatService.Core.Enums.Message;
using ChatApp.ChatService.Core.Interfaces;
using ChatApp.ChatService.Core.Mappings;
using ChatApp.ChatService.Core.RequestResponseModels.Chat;
using ChatApp.ChatService.Core.RequestResponseModels.Message;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Shared.Constants;
using Shared.EventContracts;
using Shared.Producers;

namespace ChatService.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly ILogger<IMessageService> _logger;
        private readonly IEventPublisher _eventPublisher;

        public MessageService(IMessageRepository messageRepository, ILogger<IMessageService> logger, IEventPublisher eventPublisher)
        {
            _messageRepository = messageRepository;
            _logger = logger;
            _eventPublisher = eventPublisher;
        }

        public async Task<Message> GetByIdAsync(string messageId)
        {
            return await _messageRepository.GetByIdAsync(messageId);
        }

        public async Task<ServiceResponse<List<MessageDto>>> GetMessagesByChatIdAsync(string chatId)
        {
            try
            {
                if (string.IsNullOrEmpty(chatId))
                {
                    return new ServiceResponse<List<MessageDto>>
                    {
                        Success = false,
                        Message = "Chat ID cannot be null or empty."
                    };
                }

                if (!ObjectId.TryParse(chatId, out var chatObjectId))
                {
                    return new ServiceResponse<List<MessageDto>>
                    {
                        Success = false,
                        Message = "Invalid Chat ID format."
                    };
                }

                var messages = await _messageRepository.GetMessagesByChatIdAsync(chatObjectId);

                return new ServiceResponse<List<MessageDto>>
                {
                    Success = true,
                    Message = "Messages retrieved successfully.",
                    Data = MappingToDtos.MapListOfMessagesToDto(messages)
                };
            }
            catch (Exception ex)
            {
                // Optional: log exception here
                return new ServiceResponse<List<MessageDto>>
                {
                    Success = false,
                    Message = $"An error occurred while retrieving messages. {ex}"
                };
            }
        }

        public async Task<List<Message>> GetMessagesByUserIdAsync(string userId)
        {
            return await _messageRepository.GetMessagesByUserIdAsync(userId);
        }

        public async Task<List<Message>> GetUnreadMessagesByUserIdAsync(string userId)
        {
            return await _messageRepository.GetUnreadMessagesByUserIdAsync(userId);
        }

        public async Task<ServiceResponse<MessageDto>> SendMessageAsync(SendMessageDto dto)
        {
            var message = new Message
            {
                ChatId = new ObjectId(dto.ChatId),
                SenderUsername = dto.From,
                ReceiverUsername = dto.To,
                SentAt = DateTime.UtcNow,
                Text = dto.Text,
                MessageType = Enum.Parse<MessageType>(dto.MessageType),
                IsEdited = false,
                MessageStatus = MessageStatus.Sent
            };
            try
            {
                // Log the message creation and sending process
                _logger.LogInformation("Attempting to send message {MessageId} for chat {ChatId}.", message.MessageId, message.ChatId);

                //// Send the message to the repository
                //Message createdMessage = await _messageRepository.SendMessageAsync(message);
                //MappingToDtos.MapMessageToDto(createdMessage);

                //OR
                await _messageRepository.SendMessageAsync(message);
                MessageDto data = MappingToDtos.MapMessageToDto(message);

                _eventPublisher.Publish(Exchanges.ChatMessageExchange, RoutingKeys.ChatMessageSent, new MessageSentEvent
                {
                    MessageId = message.MessageId.ToString(),
                    ChatId = message.ChatId.ToString()
                });

                return new ServiceResponse<MessageDto>
                {
                    Success = true,
                    Message = "Message sent successfully.",
                    Data = data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending message {MessageId} for chat {ChatId}.", message.MessageId, message.ChatId);
                throw new Exception("An error occurred while sending the message.", ex);
            }
        }

        public async Task MessageSendEventAsync(MessageSentEvent messageSentEvent)
        {

        }

        public async Task UpdateMessageAsync(Message message)
        {
            await _messageRepository.UpdateMessageAsync(message);
        }

        public async Task DeleteMessageAsync(string messageId)
        {
            await _messageRepository.DeleteMessageAsync(messageId);
        }

        public async Task MarkMessageAsReadAsync(string messageId)
        {
            await _messageRepository.MarkMessageAsReadAsync(messageId);
        }

        public async Task MarkChatMessagesAsReadAsync(string chatId, string userId)
        {
            await _messageRepository.MarkChatMessagesAsReadAsync(chatId, userId);
        }

        public async Task<ServiceResponse<MessageDto>> UpdateMessageStatusAsync(string messageId, MessageStatus status)
        {
            try
            {
                Message? updatedMessage = await _messageRepository.UpdateMessageStatusAsync(messageId, status);

                if (updatedMessage == null)
                {
                    return new ServiceResponse<MessageDto>
                    {
                        Success = false,
                        Message = "Message not found.",
                        Data = null
                    };
                }

                MessageDto messageDto = MappingToDtos.MapMessageToDto(updatedMessage); // <-- Assuming this method exists

                return new ServiceResponse<MessageDto>
                {
                    Success = true,
                    Message = "Message status updated successfully.",
                    Data = messageDto
                };
            }
            catch (Exception ex)
            {
                // Log exception if needed

                return new ServiceResponse<MessageDto>
                {
                    Success = false,
                    Message = $"An error occurred while updating message status: {ex.Message}",
                    Data = null
                };
            }
        }


    }
}
