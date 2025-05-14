using ChatApp.UserService.Core.Entities;
using ChatApp.UserService.Core.Enums;
using ChatApp.UserService.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Shared.EventContracts;
using Shared.Models.User;

namespace ChatApp.UserService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User2> _users;
        private readonly ILogger<IUserRepository> _logger;

        public UserRepository(IConfiguration config, IMongoClient client, ILogger<IUserRepository> logger)
        {
            var databaseName = config["MongoDbSettings:DatabaseName"];
            var collectionName = "Users";
            var database = client.GetDatabase(databaseName);
            _users = database.GetCollection<User2>(collectionName);
            _logger = logger;
        }

        #region Get User

        public async Task<User2> GetByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username && u.IsActive == true).FirstOrDefaultAsync();
        }

        public async Task<User2> GetUserByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email && u.IsActive == true).FirstOrDefaultAsync();
        }

        public async Task<List<User2>> SearchUsersAsync(string searchTerm)
        {
            var filter = Builders<User2>.Filter.And(
                Builders<User2>.Filter.Eq(u => u.IsActive, true), // ✅ Only active users
                Builders<User2>.Filter.Regex(u => u.Username, new BsonRegularExpression(searchTerm, "i")) // ✅ Username matches searchTerm (case-insensitive)
            );

            return await _users.Find(filter).ToListAsync();
        }
        #endregion

        public async Task<User2?> GetUserByResetTokenAsync(string resetToken)
        {
            return await _users.Find(u => u.PasswordResetToken == resetToken && u.IsActive == true).FirstOrDefaultAsync();
        }

        public async Task<bool> CreateUserAsync(User2 user)
        {
            try
            {
                var existingUser = await GetByUsernameAsync(user.Username);

                if (existingUser != null)
                {
                    _logger.LogWarning("User with username {Username} already exists.", user.Username);
                    throw new InvalidOperationException($"User with username {user.Username} already exists.");
                }

                user.CreatedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                user.IsActive = true;

                await _users.InsertOneAsync(user);

                _logger.LogInformation("User {Username} created successfully.", user.Username);
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user {Username}.", user.Username);
                throw new InvalidOperationException($"Error creating user {user.Username}: {ex.Message}", ex);
            }
        }

        public async Task<User2> UpdateUserAsync(UserDto user)
        {
            UserStatus userStatus = Enum.Parse<UserStatus>(user.Status ?? UserStatus.Offline.ToString(), ignoreCase: true);

            var filter = Builders<User2>.Filter.Eq(u => u.Username, user.Username);
            var update = Builders<User2>.Update
                .Set(u => u.FirstName, user.FirstName)
                .Set(u => u.MiddleName, user.MiddleName)
                .Set(u => u.LastName, user.LastName)
                .Set(u => u.Phone, user.Phone)
                .Set(u => u.ProfilePicture, user.ProfilePicture)
                .Set(u => u.Status, userStatus)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            return await _users.FindOneAndUpdateAsync(filter, update, new FindOneAndUpdateOptions<User2> { ReturnDocument = ReturnDocument.After });
        }

        public async Task DeleteUserAsync(string username)
        {
            var filter = Builders<User2>.Filter.Eq(u => u.Username, username);
            await _users.DeleteOneAsync(filter);
        }

        #region User Events Methods
        public async Task<bool> CreateUserProfileAsync(UserRegisteredEvent userEvent)
        {
            if (await GetByUsernameAsync(userEvent.Username) != null)
            {
                _logger.LogWarning("User profile creation skipped. Username {Username} already exists.", userEvent.Username);
                throw new InvalidOperationException($"User with username {userEvent.Username} already exists.");
            }

            var newUser = new User2
            {
                UserId = userEvent.UserId,
                Username = userEvent.Username,
                Email = userEvent.Email,
                Roles = userEvent.Roles,
                IsActive = userEvent.IsActive,
                CreatedAt = userEvent.CreatedAt
            };

            try
            {
                newUser.CreatedAt = DateTime.UtcNow;
                newUser.UpdatedAt = DateTime.UtcNow;
                newUser.IsActive = true;

                _logger.LogInformation("Creating user profile for {Username}.", userEvent.Username);

                await _users.InsertOneAsync(newUser);

                _logger.LogInformation("User profile created successfully for {Username}.", userEvent.Username);

                return true;
            } catch(Exception ex)
            {
                _logger.LogError(ex, "Error creating user profile for {Username}.", userEvent.Username);
                throw new InvalidOperationException($"Error creating user profile for {userEvent.Username}: {ex.Message}", ex);
            }
        }

        public async Task<bool> ChangeUsernameAsync(UsernameChangedEvent usernameChangedEvent)
        {
            var user = await GetByUsernameAsync(usernameChangedEvent.CurrentUsername) ?? throw new InvalidOperationException($"User not found with given username: {usernameChangedEvent.CurrentUsername}");

            var update = Builders<User2>.Update
                .Set(u => u.Username, usernameChangedEvent.NewUsername)
                .Set(u => u.UpdatedAt, usernameChangedEvent.ChangedAt);
            try
            {
                var result = await _users.UpdateOneAsync(u => u.Username == usernameChangedEvent.CurrentUsername, update);
                _logger.LogInformation("Username changed from {OldUsername} to {NewUsername}.", usernameChangedEvent.CurrentUsername, usernameChangedEvent.NewUsername);

                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing the username for {usernameChangedEvent.CurrentUsername}");
                throw new InvalidOperationException($"Error changing the username for {usernameChangedEvent.CurrentUsername}");
            }
        }

        public async Task<bool> UpdateEmailAsync(EmailChangedEvent emailChangedEvent)
        {
            var user = await GetByUsernameAsync(emailChangedEvent.Username) ?? throw new InvalidOperationException($"User not found with given username: {emailChangedEvent.Username}");

            var update = Builders<User2>.Update
                .Set(u => u.Email, emailChangedEvent.NewEmail)
                .Set(u => u.UpdatedAt, emailChangedEvent.UpdatedAt);
            try
            {
                _logger.LogInformation("Updating email for {Username} to {NewEmail}.", emailChangedEvent.Username, emailChangedEvent.NewEmail);
                var result = await _users.UpdateOneAsync(u => u.Username == emailChangedEvent.Username, update);
                
                _logger.LogInformation("Email updated for {Username}.", emailChangedEvent.Username);
                return result.ModifiedCount > 0;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error updating the email for {emailChangedEvent.Username}");
                throw new InvalidOperationException($"Error updating the email for {emailChangedEvent.Username}");
            }
        }

        public async Task<bool> DeactivateUserAsync(UserDeletedEvent userDeletedEvent)
        {
            var update = Builders<User2>.Update
                .Set(u => u.IsActive, false)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            try
            {
                var user = await GetByUsernameAsync(userDeletedEvent.Username) ?? throw new InvalidOperationException($"User not found with given username: {userDeletedEvent.Username}");
                _logger.LogInformation("Deactivating user {Username}.", userDeletedEvent.Username);

                var result = await _users.UpdateOneAsync(u => u.Username == userDeletedEvent.Username, update);
                _logger.LogInformation("User {Username} marked as inactive.", userDeletedEvent.Username);

                return result.ModifiedCount > 0;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Error deactivating the user {userDeletedEvent.Username}");
                throw new InvalidOperationException($"Error deactivating the user {userDeletedEvent.Username}");
            }
        }
        #endregion

        public async Task<List<User2>> GetUsersBatchAsync(List<string> usernames)
        {
            if (usernames == null || usernames.Count == 0)
            {
                _logger.LogWarning("Empty usernames list provided for batch fetch.");
                return [];
            }

            var filter = Builders<User2>.Filter.In(u => u.Username, usernames);
            var result = await _users.Find(filter).ToListAsync();

            _logger.LogInformation("Fetched {Count} users for batch.", result.Count);
            return result;
        }

    }
}