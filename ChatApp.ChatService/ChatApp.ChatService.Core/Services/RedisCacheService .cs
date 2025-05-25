using ChatApp.ChatService.Core.DTOs.Message;
using ChatApp.ChatService.Core.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatApp.ChatService.Core.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        private const string UnsentPrefix = "unsent:";

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _database = redis.GetDatabase();
            _logger = logger;
        }

        public async Task AddUnsentMessageAsync(string username, MessageDto message)
        {
            var key = $"{UnsentPrefix}{username}";
            string serializedMessage = JsonSerializer.Serialize(message);

            await _database.ListRightPushAsync(key, serializedMessage);
            _logger.LogInformation("🔁 Message queued for offline user: {Username}", username);
        }

        public async Task<List<MessageDto>> GetUnsentMessagesAsync(string username)
        {
            var key = $"{UnsentPrefix}{username}";
            var values = await _database.ListRangeAsync(key);

            var messages = values
                .Select(v => JsonSerializer.Deserialize<MessageDto>(v!))
                .Where(m => m != null)
                .ToList()!;

            return messages;
        }

        public async Task ClearUnsentMessagesAsync(string username)
        {
            var key = $"{UnsentPrefix}{username}";
            await _database.KeyDeleteAsync(key);
            _logger.LogInformation("🧹 Cleared unsent messages for user: {Username}", username);
        }
    }
}
