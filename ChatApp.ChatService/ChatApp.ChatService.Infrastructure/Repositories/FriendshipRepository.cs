using ChatApp.ChatService.Core.Entities.Friendship;
using ChatApp.ChatService.Core.Enums.Friend;
using ChatApp.ChatService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace ChatApp.ChatService.Infrastructure.Repositories
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IMongoCollection<Friendship> _friendshipCollection;
        private readonly ILogger<IFriendshipRepository> _logger;
        private readonly string _internalApiSecret;

        public FriendshipRepository(IConfiguration config, IMongoClient client, ILogger<IFriendshipRepository> logger, HttpClient httpClient)
        {
            var databaseName = config["MongoDbSettings:DatabaseName"];
            var collectionName = "Chats";

            var database = client.GetDatabase(databaseName);
            _friendshipCollection = database.GetCollection<Friendship>(collectionName);
            _logger = logger;

            _httpClient = httpClient;
            _internalApiSecret = config["InternalApi:Secret"] ?? "your-fallback-secret";
        }

        public async Task<Friendship?> GetFriendshipAsync(string username1, string username2)
        {
            try
            {
                return await _friendshipCollection
                    .Find(f => (f.SenderUsername == username1 && f.RecipientUsername == username2) ||
                               (f.SenderUsername == username2 && f.RecipientUsername == username1))
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch friendship between '{Username1}' and '{Username2}'", username1, username2);
                throw; // propagate to service
            }
        }

        public async Task<List<Friendship>> GetAcceptedFriendshipsAsync(string username)
        {
            try
            {
                _logger.LogInformation("Fetching accepted friendships for user '{Username}'.", username);

                return await _friendshipCollection
                    .Find(f => (f.SenderUsername == username || f.RecipientUsername == username) && f.Status == FriendRequestStatus.Accepted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching accepted friendships for user '{Username}'.", username);
                throw; // Important: rethrow so that service layer handles it properly.
            }
        }

        public async Task<List<Friendship>> GetPendingRequestsAsync(string username)
        {
            try
            {
                _logger.LogInformation("Fetching pending friend requests for user '{Username}'.", username);

                return await _friendshipCollection
                    .Find(f => f.RecipientUsername == username && f.Status == FriendRequestStatus.Pending)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching pending friend requests for user '{Username}'.", username);
                throw; // Important: rethrow to let service decide what to do
            }
        }

        public async Task CreateAsync(Friendship friendship)
        {
            try
            {
                await _friendshipCollection.InsertOneAsync(friendship);
                _logger.LogInformation("Friendship record created with Id: '{FriendshipId}'", friendship.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create friendship record for '{SenderUsername}' -> '{RecipientUsername}'", friendship.SenderUsername, friendship.RecipientUsername);
                throw; // propagate to service
            }
        }

        public async Task UpdateAsync(Friendship friendship)
        {
            try
            {
                var filter = Builders<Friendship>.Filter.Eq(f => f.Id, friendship.Id);
                await _friendshipCollection.ReplaceOneAsync(filter, friendship);

                _logger.LogInformation("Friendship with Id '{FriendshipId}' updated successfully.", friendship.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update friendship with Id '{FriendshipId}'", friendship.Id);
                throw; // important: rethrow to let service layer handle it
            }
        }

        public async Task DeleteAsync(string friendshipId)
        {
            try
            {
                var filter = Builders<Friendship>.Filter.Eq(f => f.Id, friendshipId);
                var result = await _friendshipCollection.DeleteOneAsync(filter);

                if (result.DeletedCount > 0)
                {
                    _logger.LogInformation("Friendship with Id '{FriendshipId}' successfully deleted.", friendshipId);
                }
                else
                {
                    _logger.LogWarning("No friendship found with Id '{FriendshipId}' to delete.", friendshipId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete friendship with Id '{FriendshipId}'", friendshipId);
                throw; // propagate
            }
        }

        // Get friend requests with specific status for a user
        public async Task<List<Friendship>> GetFriendRequestsAsync(string userId, FriendRequestStatus status)
        {
            return await _friendshipCollection
                .Find(f => f.RecipientUsername == userId && f.Status == status)
                .ToListAsync();
        }

    }
}
