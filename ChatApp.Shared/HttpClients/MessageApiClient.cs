using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Shared.HttpClients.Interfaces;
using System.Buffers.Text;

namespace Shared.HttpClients
{
    public class MessageApiClient : IMessageApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IMessageApiClient> _logger;
        private readonly string _internalApiSecret;
        private readonly string BASE_URL;

        public MessageApiClient(HttpClient httpClient, ILogger<IMessageApiClient> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _internalApiSecret = configuration["InternalApi:Secret"] ?? "fall-back-secret";
            BASE_URL = configuration["Services:ChatService"] ?? "http://localhost:5003";
        }

        public async Task<string> GetMessagesByChatId(string chatId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{BASE_URL}/api/message/chat/{chatId}");
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
