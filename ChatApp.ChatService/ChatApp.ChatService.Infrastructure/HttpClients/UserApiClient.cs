using ChatApp.ChatService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChatApp.ChatService.Infrastructure.HttpClients
{
    public class UserApiClient: IUserApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IUserApiClient> _logger;
        private readonly string _internalApiSecret;

        public UserApiClient(HttpClient httpClient, ILogger<IUserApiClient> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _internalApiSecret = configuration["InternalApi:Secret"] ?? "fall-back-secret";
        }
        public async Task<string> GetUsersByUsernamesBatch(string usernames)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5002/api/user/batch?usernames={usernames}");
                request.Headers.Add("X-Internal-Secret", _internalApiSecret);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch participant details: {StatusCode}", response.StatusCode);
                    return string.Empty;
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching user details from UserService for '{Usernames}'", usernames);
                return string.Empty; // safest fallback
            }
        }

        public async Task<string> SearchUsersByUsername(string searchTerm)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:5002/api/user/search?SearchTerm={searchTerm}");
            request.Headers.Add("X-Internal-Secret", _internalApiSecret);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch user details: {StatusCode}", response.StatusCode);
                return string.Empty;
            }

            _logger.LogInformation("Successfully fetched participant details for '{SearchTerm}'", searchTerm);

            return await response.Content.ReadAsStringAsync();
        }
    }
}
