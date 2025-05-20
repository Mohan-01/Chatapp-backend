using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using ChatApp.ChatService.Core.Interfaces;
using Shared.HttpClients.Interfaces;
using Shared.Models.User;
using ChatApp.ChatService.Core.RequestResponseModels.Message;
using ChatApp.ChatService.Core.DTOs.Message;
using ChatApp.ChatService.Core.RequestResponseModels.Chat;
using System.Threading.Tasks;
using ChatApp.ChatService.Core.Enums.Message;

namespace ChatApp.ChatService.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService; // Service for managing messages in MongoDB
        //private readonly IRedisCacheService _redisCacheService; // NEW: Redis Service
        private readonly IUserApiClient _userApiClient; // Service for managing users in MongoDB
        private readonly ILogger<ChatHub> _logger;

        private static readonly ConcurrentDictionary<string, ConnectedUser> ConnectedUsers = new(); // Store connected users

        public ChatHub(IMessageService messageService, IUserApiClient userApiClient, ILogger<ChatHub> logger)
        {
            _messageService = messageService;
            //_redisCacheService = redisCacheService; // Inject Redis Cache Service
            _userApiClient = userApiClient;
            _logger = logger;
            _logger.LogInformation("ChatHub initiated");
        }

        // Represents connected user details
        private class ConnectedUser
        {
            required public string ConnectionId { get; set; }
            required public string UserName { get; set; }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"ConnectionId: {Context.ConnectionId}");

            // Print the UserIdentifier
            _logger.LogInformation($"UserIdentifier: {Context.UserIdentifier}");

            // Print all claims if using authentication
            var claimsPrincipal = Context.User;
            _logger.LogInformation($"ClaimsPrincipal: {claimsPrincipal}");
            if (claimsPrincipal != null)
            {
                foreach (var claim in claimsPrincipal.Claims)
                {
                    _logger.LogInformation($"ClaimType: {claim.Type}, ClaimValue: {claim.Value}");
                }
            }
            else
            {
                _logger.LogInformation("No user claims found.");
            }

            // Print additional HttpContext details
            if (Context.GetHttpContext != null)
            {
                _logger.LogInformation($"HttpContext: {Context.GetHttpContext}");
                _logger.LogInformation($"Request Headers:");
            }
            else
            {
                _logger.LogInformation("HttpContext is null.");
            }

            var username = Context.User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
            {
                _logger.LogInformation($"user: {username}");
                string content = await _userApiClient.GetUsersByUsernamesBatch(username); // Fetch user details

                if(string.IsNullOrEmpty(content))
                {
                    _logger.LogInformation("User not found.");
                    return;
                }

                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ServiceResponse<List<UserDto>>>(content);

                if(response == null || !response.Success || response.Data == null)
                {
                    _logger.LogInformation("Failed to get user information from service");
                    return;
                }

                var user = response.Data.FirstOrDefault();
                
                if (user == null)
                {
                    _logger.LogInformation("User not found.");
                    return;
                }
                
                var connectedUser = new ConnectedUser
                {
                    ConnectionId = Context.ConnectionId,
                    UserName = user.Username
                };

                ConnectedUsers[username] = connectedUser;

                // Notify other users about the connection
                await Clients.AllExcept(Context.ConnectionId).SendAsync("UserConnected", connectedUser);

                // ✅ Retry unsent messages for reconnected user
                //var unsentMessages = await _redisCacheService.GetUnsentMessages(username);
                //foreach (var message in unsentMessages)
                //{
                //    await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", message);
                //}
                
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var username = Context.UserIdentifier;
            if (username != null && ConnectedUsers.TryRemove(username, out var disconnectedUser))
            {
                // Notify other users about the disconnection
                await Clients.All.SendAsync("UserDisconnected", disconnectedUser);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(SendMessageDto sendMessageDto)
        {
            _logger.LogInformation($"SendMessage called with: {JsonSerializer.Serialize(sendMessageDto)}");

            try
            {
                //if (await _redisCacheService.IsRateLimited(messageDto.From, 10, TimeSpan.FromSeconds(10)))
                //{
                //    await Clients.Caller.SendAsync("Error", "Rate limit exceeded. Please wait before sending more messages.");
                //    _logger.LogInformation("Rate limit exceeded!");
                //    return;
                //}

                _logger.LogInformation("SendMessage in ChatHub");

                ServiceResponse<MessageDto> response = await _messageService.SendMessageAsync(sendMessageDto);

                if(!response.Success)
                {
                    _logger.LogInformation("Something went wrong when sendmessageasync chathub");
                    throw new Exception(response.Message);
                }

                MessageDto messageDto = response.Data;

                if(messageDto == null)
                {
                    _logger.LogInformation("MessageDto got null");
                    throw new Exception("Something went wrong");
                }
                
                _logger.LogInformation("Message saved in mongodb");

                _logger.LogInformation($"Checking if recipient {messageDto.To} is connected");
                foreach (var kvp in ConnectedUsers)
                {
                    _logger.LogInformation($"Connected: {kvp.Key} => {kvp.Value.ConnectionId}");
                }

                _logger.LogInformation($"messageDto.To = {messageDto.To}, Keys in ConnectedUsers: {string.Join(", ", ConnectedUsers.Keys)}");

                // Check if the recipient is online
                if (ConnectedUsers.TryGetValue(messageDto.To, out var recipient))
                {
                    // Deliver the message in real time
                    await Clients.Client(recipient.ConnectionId).SendAsync("ReceiveMessage", messageDto);
                    _logger.LogInformation("Message delivered ReceiveMessage");

                    // Mark the message as delivered
                    await _messageService.UpdateMessageStatusAsync(messageDto.MessageId, MessageStatus.Delivered);
                    messageDto.MessageStatus = "Delivered";
                    _logger.LogInformation("Updated status to delivered");
                }
                else
                {
                    // ✅ Cache unsent message in Redis
                    //await _redisCacheService.SaveUnsentMessage(message.ReceiverUsername, JsonSerializer.Serialize(messageDto));
                }

                // Notify the sender about the message status
                await Clients.Caller.SendAsync("MessageStatusUpdated", messageDto);
                _logger.LogInformation("Notify the status");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Failed to send message. {ex}");
                await Clients.Caller.SendAsync("Error", $"Failed to send message. {ex}");
            }
        }

        public async Task MarkMessageAsRead(ChangeMessageStatus Message)
        {
            _logger.LogInformation($"MarkMessageAsRead called with: {Message.MessageId}");
            try
            {
                ServiceResponse<MessageDto> response = await _messageService.UpdateMessageStatusAsync(Message.MessageId, MessageStatus.Seen);
                // Send status update to the sender and recipient
                await Clients.User(response.Data.From).SendAsync("MessageStatusUpdated", response.Data);
                //await Clients.User(Message.To).SendAsync("MessageStatusUpdated", Message);
                await Clients.Caller.SendAsync("MessageStatusUpdated", response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Failed to mark message as read. {ex}");
                await Clients.Caller.SendAsync("Error", $"Failed to mark message as read. {ex}");

            }
        }
    }
}