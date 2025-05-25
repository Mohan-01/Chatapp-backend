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
        private readonly IRedisCacheService _redisCacheService; // Injected


        private static readonly ConcurrentDictionary<string, ConnectedUser> ConnectedUsers = new(); // Store connected users

        public ChatHub(IMessageService messageService, IUserApiClient userApiClient, ILogger<ChatHub> logger, IRedisCacheService redisCacheService)
        {
            _messageService = messageService;
            //_redisCacheService = redisCacheService; // Inject Redis Cache Service
            _userApiClient = userApiClient;
            _redisCacheService = redisCacheService;
            _logger = logger;
            _logger.LogInformation("ChatHub initiated");
        }

        // Represents connected user details
        private class ConnectedUser
        {
            required public string ConnectionId { get; set; }
            required public string Username { get; set; }
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
                    Username = user.Username
                };

                ConnectedUsers[username] = connectedUser;

                // Notify other users about the connection
                await Clients.AllExcept(Context.ConnectionId).SendAsync("UserConnected", connectedUser);

                // ✅ Retry unsent messages for reconnected user
                var unsentMessages = await _redisCacheService.GetUnsentMessagesAsync(username);
                foreach (var message in unsentMessages)
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", message, "system-retry");
                    await _redisCacheService.ClearUnsentMessagesAsync(username);
                    _logger.LogInformation("📤 Retried message {MessageId} for {Username}", message.MessageId, username);
                }

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

        /*
        
            1. Client A -> Server - Done
            2. Server save to mongodb - Done
            2.1 Broadcast to client B - Done
            3. Server notify Client A with sent status - Done
            4. Client B upon successfully received ack server with (delivered status)
            5. Server update status to mongodb
            6. Server notifies Client A with the status
            7. Client B open/focus the message sends (seen/read) status to server
            8. Server update status to mongodb
            9. Server notify Client A with read status

         */

        // 1️⃣ Client A triggers this to send a message
        public async Task SendMessage(SendMessageDto sendMessageDto, string clientId)
        {
            _logger.LogInformation("📨 SendMessage called: {Payload}", JsonSerializer.Serialize(sendMessageDto));

            try
            {
                var savedMessage = await _messageService.SendMessageAsync(sendMessageDto);

                if (!savedMessage.Success || savedMessage.Data == null)
                    throw new Exception(savedMessage.Message ?? "Failed to save message");

                MessageDto message = savedMessage.Data;

                // 2️⃣ Deliver to recipient if online
                if (ConnectedUsers.TryGetValue(message.To, out var recipient))
                {
                    await Clients.Client(recipient.ConnectionId).SendAsync("ReceiveMessage", message, clientId);
                    _logger.LogInformation("📤 Delivered to {Recipient}", message.To);
                }
                else
                {
                    _logger.LogInformation("🕳️ Recipient {Recipient} not online.", message.To);
                    // Optionally queue or cache for retry
                    await _redisCacheService.AddUnsentMessageAsync(message.To, message);

                }

                // ✅ Respond to Client A with "Sent" status (DO NOT say Delivered)
                message.MessageStatus = MessageStatus.Sent.ToString();
                await Clients.Caller.SendAsync("MessageStatusUpdated", message, clientId);

                _logger.LogInformation("✅ Client A notified with Sent status");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in SendMessage");
                await Clients.Caller.SendAsync("Error", $"Failed to send message. {ex.Message}");
            }
        }

        // 3️⃣ Client B explicitly triggers this after receiving message
        public async Task MarkMessageAsDelivered(ChangeMessageStatus request)
        {
            _logger.LogInformation("📦 MarkMessageAsDelivered called for: {MessageId}", request.MessageId);

            try
            {
                var result = await _messageService.UpdateMessageStatusAsync(request.MessageId, MessageStatus.Delivered);

                if (!result.Success || result.Data == null)
                    throw new Exception(result.Message ?? "Status update failed");

                var message = result.Data;

                // Notify Client A (sender)
                await Clients.User(message.From).SendAsync("MessageStatusUpdated", message);
                await Clients.User(message.To).SendAsync("MessageStatusUpdated", message);

                _logger.LogInformation("📬 Delivered status sent to Client A: {MessageId}", message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to update delivery status");
                await Clients.Caller.SendAsync("Error", $"Failed to mark as delivered. {ex.Message}");
            }
        }

        // 4️⃣ Client B triggers this after reading the message
        public async Task MarkMessageAsRead(ChangeMessageStatus request)
        {
            _logger.LogInformation("👁️ MarkMessageAsRead called for: {MessageId}", request.MessageId);

            try
            {
                var result = await _messageService.UpdateMessageStatusAsync(request.MessageId, MessageStatus.Seen);

                if (!result.Success || result.Data == null)
                    throw new Exception(result.Message ?? "Status update failed");

                var message = result.Data;

                // Notify Client A (sender)
                await Clients.User(message.From).SendAsync("MessageStatusUpdated", message);
                await Clients.User(message.To).SendAsync("MessageStatusUpdated", message);

                _logger.LogInformation("👀 Seen status sent to Client A: {MessageId}", message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to update seen status");
                await Clients.Caller.SendAsync("Error", $"Failed to mark as read. {ex.Message}");
            }
        }

        //public async Task MarkMessageAsRead(ChangeMessageStatus Message)
        //{
        //    _logger.LogInformation($"MarkMessageAsRead called with: {Message.MessageId}");
        //    try
        //    {
        //        ServiceResponse<MessageDto> response = await _messageService.UpdateMessageStatusAsync(Message.MessageId, MessageStatus.Seen);
        //        // Send status update to the sender and recipient
        //        await Clients.User(response.Data.From).SendAsync("MessageStatusUpdated", response.Data);
        //        //await Clients.User(Message.To).SendAsync("MessageStatusUpdated", Message);
        //        await Clients.Caller.SendAsync("MessageStatusUpdated", response.Data);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogInformation($"Failed to mark message as read. {ex}");
        //        await Clients.Caller.SendAsync("Error", $"Failed to mark message as read. {ex}");

        //    }
        //}
    }
}