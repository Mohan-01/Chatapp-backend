using ChatApp.ChatService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChatApp.ChatService.Infrastructure.HttpClients
{
    public class ChatApiClient: IChatApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IChatApiClient> _logger;
        private readonly string _internalApiSecret;

        public ChatApiClient(HttpClient httpClient, ILogger<IChatApiClient> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _internalApiSecret = configuration["InternalApi:Secret"] ?? "fall-back-secret";
        }

        public async Task<string> GetChatByUsernamesAsync(string sender, string receiver)
        {
            // Call ChatService to get chat details
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5003/api/chat/{sender}/{receiver}");
            request.Headers.Add("X-Internal-Secret", _internalApiSecret);

            var response = await _httpClient.SendAsync(request);
            string content = "";

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch chat details: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Chat details failed to fetch");
            }
            else
            {
                content = await response.Content.ReadAsStringAsync();
                //_logger.LogInformation("Chat details fetched successfully with count: {count}", newChat.ParticipantsDetails.Count);
            }
            return content;
        }
    }
}
