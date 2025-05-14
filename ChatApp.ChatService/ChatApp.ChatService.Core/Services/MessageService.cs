using ChatApp.ChatService.Core.DTOs.Message;
using ChatApp.ChatService.Core.Entities.Message;
using ChatApp.ChatService.Core.Enums.Message;
using ChatApp.ChatService.Core.Interfaces;
using ChatApp.ChatService.Core.Mappings;
using ChatApp.ChatService.Core.RequestResponseModels.Chat;
using ChatApp.ChatService.Core.RequestResponseModels.Message;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace ChatService.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly ILogger<IMessageService> _logger;

        public MessageService(IMessageRepository messageRepository, ILogger<IMessageService> logger)
        {
            _messageRepository = messageRepository;
            _logger = logger;
        }

        public async Task<Message> GetByIdAsync(string messageId)
        {
            return await _messageRepository.GetByIdAsync(messageId);
        }

        public async Task<IEnumerable<Message>> GetMessagesByChatIdAsync(string chatId)
        {
            return await _messageRepository.GetMessagesByChatIdAsync(chatId);
        }

        public async Task<IEnumerable<Message>> GetMessagesByUserIdAsync(string userId)
        {
            return await _messageRepository.GetMessagesByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Message>> GetUnreadMessagesByUserIdAsync(string userId)
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

                // Send the message to the repository
                Message createdMessage = await _messageRepository.SendMessageAsync(message);
                MappingToDtos.MapMessageToDto(createdMessage);

                //OR
                await _messageRepository.SendMessageAsync(message);
                MessageDto data = MappingToDtos.MapMessageToDto(message);
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

        public async Task UpdateMessageStatusAsync(string messageId, string status)
        {
            if (Enum.TryParse<MessageStatus>(status, out var parsedStatus))
            {
                await _messageRepository.UpdateMessageStatusAsync(messageId, parsedStatus);
            }
            else
            {
                throw new ArgumentException($"Invalid message status: {status}", nameof(status));
            }
        }

    }
}
