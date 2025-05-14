using Microsoft.Extensions.Logging;
using ChatApp.ChatService.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ChatApp.ChatService.Infrastructure.HttpClients
{
    public class MessageApiClient : IMessageApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IMessageApiClient> _logger;
        private readonly string _internalApiSecret;

        public MessageApiClient(HttpClient httpClient, ILogger<IMessageApiClient> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _internalApiSecret = configuration["InternalApi:Secret"] ?? "fall-back-secret";
        }

        public async Task<string> GetMessagesByChatId(string chatId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5003/api/message/chat/{chatId}");
            request.Headers.Add("X-Internal-Secret", _internalApiSecret);

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch messages: {StatusCode}", response.StatusCode);
                return string.Empty;
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
