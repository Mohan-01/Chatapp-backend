using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatApp.ChatService.Infrastructure.SignalR
{
    public class CustomUserIdProvider: IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // You can log all claims to debug
            var userId = connection.User?.FindFirst(ClaimTypes.Name)?.Value;
            return userId;
        }
    }
}
