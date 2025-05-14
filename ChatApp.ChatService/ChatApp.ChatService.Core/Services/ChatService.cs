using ChatService.Entities.Chat;
using MongoDB.Bson;
using ChatApp.ChatService.Core.Interfaces;
using ChatApp.ChatService.Core.Enums.Chat;
using ChatApp.ChatService.Core.Entities.Message;
using ChatApp.ChatService.Core.RequestResponseModels.Chat;
using DnsClient.Internal;
using Microsoft.Extensions.Logging;
using Shared.Models.User;
using Newtonsoft.Json;
using ChatApp.ChatService.Core.DTOs.Chat;
using ChatApp.ChatService.Core.Mappings;
using MongoDB.Driver;
using ChatApp.ChatService.Core.Exceptions;

namespace ChatApp.ChatService.Core.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILogger<IChatService> _logger;
        private readonly IUserApiClient _userApiClient;
        private readonly IMessageApiClient _messageApiClient;
        private readonly IMongoClient _mongoClient;

        public ChatService(
            IChatRepository chatRepository, 
            ILogger<IChatService> logger, 
            IUserApiClient userApiClient, 
            IMongoClient mongoClient,
            IMessageApiClient messageApiClient)
        {
            _chatRepository = chatRepository;
            _logger = logger;
            _userApiClient = userApiClient;
            _messageApiClient = messageApiClient;
            _mongoClient = mongoClient;
        }


        // DONE
        // Create a new one-to-one chat
        public async Task<ServiceResponse<PrivateChatDto>> CreateOneToOneChatAsync(string username1, string username2)
        {
            using var session = await _mongoClient.StartSessionAsync();
            session.StartTransaction();

            try
            {
                // Fetch participants from UserService
                var content = await _userApiClient.GetUsersByUsernamesBatch($"{username1},{username2}");
                var serviceResponse = JsonConvert.DeserializeObject<ServiceResponse<List<UserDto>>>(content);

                if (serviceResponse == null || !serviceResponse.Success || serviceResponse.Data == null)
                {
                    _logger.LogWarning("Failed to fetch participants while creating chat between {Username1} and {Username2}.", username1, username2);
                    await session.AbortTransactionAsync();
                    return new ServiceResponse<PrivateChatDto> { Success = false, Message = "Failed to fetch participant details." };
                }

                Chat? existingChat = null;

                try
                {
                    existingChat = await _chatRepository.GetChatByUsernamesAsync(username1, username2);
                }
                catch (NotFoundException)
                {
                    // Chat not found is OK - we will create new one
                    _logger.LogInformation("No existing chat found between {Username1} and {Username2}. A new chat will be created.", username1, username2);
                }

                if (existingChat != null)
                {
                    await session.AbortTransactionAsync();
                    // Map to DTO
                    PrivateChatDto existingChatDto = MappingToDtos.MapPrivateChatToDto(existingChat);
                    return new ServiceResponse<PrivateChatDto> { Success = true, Message = "Chat already exists.", Data = existingChatDto };
                }

                // Create new Chat object
                var newChat = new Chat
                {
                    ChatType = ChatType.Private,
                    ChatStatus = ChatStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    Participants = [username1, username2],
                    ParticipantsDetails = serviceResponse.Data,
                };

                // Insert into DB
                var createdChat = await _chatRepository.CreateOneToOneChatAsync(session, newChat);

                await session.CommitTransactionAsync();

                if (createdChat == null)
                {
                    _logger.LogWarning("Failed to create chat between {Username1} and {Username2}.", username1, username2);
                    return new ServiceResponse<PrivateChatDto> { Success = false, Message = "Failed to create chat." };
                }

                // Map to DTO
                PrivateChatDto chatDto = MappingToDtos.MapPrivateChatToDto(createdChat);

                return new ServiceResponse<PrivateChatDto>
                {
                    Success = true,
                    Message = "Chat created successfully.",
                    Data = chatDto
                };
            }
            catch (Exception ex)
            {
                await session.AbortTransactionAsync();
                _logger.LogError(ex, "Failed to create chat between {Username1} and {Username2}.", username1, username2);
                return new ServiceResponse<PrivateChatDto>
                {
                    Success = false,
                    Message = "An unexpected error occurred."
                };
            }
        }

        // DONE
        public async Task<ServiceResponse<PrivateChatDto>> GetChatByIdAsync(string chatId)
        {
            _logger.LogInformation("Service call to fetch chat details for chatId: {ChatId}", chatId);

            try
            {
                // Get chat details from the repository (MongoDB)
                var chat = await _chatRepository.GetChatByIdAsync(chatId);

                // If chat is not found, return a response indicating no chat was found
                if (chat == null)
                {
                    _logger.LogWarning("No chat found for chatId: {ChatId}", chatId);
                    return new ServiceResponse<PrivateChatDto>
                    {
                        Success = false,
                        Message = $"No chat found with id {chatId}",
                        Data = null
                    };
                }

                // Fetch participant details from User API
                string userContent = await _userApiClient.GetUsersByUsernamesBatch(string.Join(",", chat.Participants));
                ServiceResponse<List<UserDto>>? response = JsonConvert.DeserializeObject<ServiceResponse<List<UserDto>>>(userContent);

                if (response == null || response.Data == null)
                {
                    _logger.LogWarning("Failed to fetch user details for chat {ChatId}.", chatId);
                    return new ServiceResponse<PrivateChatDto>
                    {
                        Success = false,
                        Message = $"Failed to fetch user details for chat {chatId}.",
                        Data = null
                    };
                }

                chat.ParticipantsDetails = response.Data;

                // Fetch messages from Message API
                var messageContent = await _messageApiClient.GetMessagesByChatId(chatId);
                var messages = JsonConvert.DeserializeObject<List<Message>>(messageContent);

                if (messages == null)
                {
                    _logger.LogWarning("Failed to fetch messages for chat {ChatId}.", chatId);
                    messages = [];
                    //return new ServiceResponse<PrivateChatDto>
                    //{
                    //    Success = false,
                    //    Message = $"Failed to fetch messages for chat {chatId}.",
                    //    Data = null
                    //};
                }

                chat.Messages = messages;

                // Map and return as DTO
                PrivateChatDto chatDto = MappingToDtos.MapPrivateChatToDto(chat);

                return new ServiceResponse<PrivateChatDto>
                {
                    Success = true,
                    Message = "Chat fetched successfully.",
                    Data = chatDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching chat by chatId: {ChatId} in service.", chatId);
                return new ServiceResponse<PrivateChatDto>
                {
                    Success = false,
                    Message = "An error occurred while fetching the chat details.",
                    Data = null
                };
            }
        }

        //DONE
        // Get a chat by user ids (username1 and username2)
        public async Task<ServiceResponse<PrivateChatDto>> GetChatByUsernamesAsync(string username1, string username2)
        {
            _logger.LogInformation("Getting chat between {Username1} and {Username2} from repository.", username1, username2);

            var chat = await _chatRepository.GetChatByUsernamesAsync(username1, username2);
            if (chat == null)
            {
                _logger.LogWarning("Chat not found between {Username1} and {Username2}.", username1, username2);
                throw new NotFoundException($"Chat not found between {username1} and {username2}.");
            }

            // Fetch ParticipantsDetails
            string participants = await _userApiClient.GetUsersByUsernamesBatch(string.Join(",", chat.Participants));
            ServiceResponse<List<UserDto>>? response = JsonConvert.DeserializeObject<ServiceResponse<List<UserDto>>>(participants);
            if (response == null || !response.Success || response.Data == null)
            {
                _logger.LogError("Failed to fetch participants details for chat {ChatId}.", chat.ChatId);
                throw new ApplicationException($"Unable to fetch participants details for chat {chat.ChatId}.");
            }

            chat.ParticipantsDetails = response.Data;

            // Fetch Messages
            var messageContent = await _messageApiClient.GetMessagesByChatId(chat.ChatId.ToString());
            var messages = JsonConvert.DeserializeObject<List<Message>>(messageContent);

            if (messages == null)
            {
                _logger.LogError("Failed to fetch messages for chat {ChatId}.", chat.ChatId);
                messages = [];
            }
            chat.Messages = messages;

            // Map to DTO
            PrivateChatDto chatDto = MappingToDtos.MapPrivateChatToDto(chat);

            return new ServiceResponse<PrivateChatDto> { 
                Success = true,
                Message = "Chat retrieved successfully.",
                Data = chatDto
            };
        }

        // DONE
        // Get all chats for a specific user (filter by ChatType: Active or Archived)
        public async Task<ServiceResponse<List<PrivateChatDto>>> GetChatsByUsernameAsync(string username, ChatStatus chatStatus = ChatStatus.Active)
        {
            _logger.LogInformation("Fetching chats for user {Username} with status {ChatStatus}.", username, chatStatus);

            try
            {
                var chats = await _chatRepository.GetChatsByUsernameAsync(username, chatStatus);

                if (chats == null || chats.Count == 0)
                {
                    _logger.LogWarning("No chats found for user {Username} with status {ChatStatus}.", username, chatStatus);
                    return new ServiceResponse<List<PrivateChatDto>>
                    {
                        Success = true,
                        Message = "No chats found.",
                        Data = []
                    };
                }

                // Fetch participants' details for all chats
                foreach (var chat in chats)
                {
                    try
                    {
                        var content = await _userApiClient.GetUsersByUsernamesBatch(string.Join(",", chat.Participants));
                        var serviceResponse = JsonConvert.DeserializeObject<ServiceResponse<List<UserDto>>>(content);

                        if (serviceResponse != null && serviceResponse.Success && serviceResponse.Data != null)
                        {
                            chat.ParticipantsDetails = serviceResponse.Data;
                        }
                        else
                        {
                            _logger.LogWarning("Failed to fetch participant details for Chat ID {ChatId}.", chat.ChatId);
                            throw new ApplicationException($"Failed to fetch participant details for chat {chat.ChatId}.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error fetching participant details for Chat ID {ChatId}.", chat.ChatId);
                        throw new ApplicationException($"Failed to fetch participant details for chat {chat.ChatId}.", ex);
                    }
                }

                var chatDtos = chats.Select(MappingToDtos.MapPrivateChatToDto).ToList();

                return new ServiceResponse<List<PrivateChatDto>>
                {
                    Success = true,
                    Message = "Chats retrieved successfully.",
                    Data = chatDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching chats for user {Username}.", username);
                throw new ApplicationException($"An unexpected error occurred while fetching chats for user {username}.", ex);
            }
        }

        // DONE
        // Update the status of the chat (active/archived)
        public async Task<ServiceResponse<PrivateChatDto>> UpdateChatStatusAsync(string chatId, ChatStatus chatStatus)
        {
            try
            {
                // Call repository to update the chat status in DB
                var updatedChat = await _chatRepository.UpdateChatStatusAsync(chatId, chatStatus);

                if (updatedChat == null)
                {
                    return new ServiceResponse<PrivateChatDto>
                    {
                        Success = false,
                        Message = $"No chat found with chatId: {chatId} or it may already have the status {chatStatus.ToString()}.",
                        Data = null
                    };
                }

                // Map and return the updated chat as DTO
                var chatDto = MappingToDtos.MapPrivateChatToDto(updatedChat);

                return new ServiceResponse<PrivateChatDto>
                {
                    Success = true,
                    Message = "Chat status updated successfully.",
                    Data = chatDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating chat status for chatId: {ChatId}.", chatId);
                return new ServiceResponse<PrivateChatDto>
                {
                    Success = false,
                    Message = "An error occurred while updating the chat status.",
                    Data = null
                };
            }
        }


        // Update the list of messages in a chat
        public async Task UpdateChatMessagesAsync(ObjectId chatId, Message messages)
        {
            await _chatRepository.UpdateChatMessagesAsync(chatId, messages);
        }

        // Archive a chat (mark as archived)
        public async Task ArchiveChatAsync(ObjectId chatId)
        {
            await _chatRepository.ArchiveChatAsync(chatId);
        }
    }
}
