using AuthService.Core.Interfaces;
using AuthService.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Shared.EventContracts;

namespace AuthService.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IMongoCollection<AuthUser> _users;
        private readonly ILogger<IAuthRepository> _logger;

        public AuthRepository(IConfiguration config, IMongoClient client, ILogger<IAuthRepository> logger)
        {
            var databaseName = config["MongoDbConfig:DatabaseName"];
            var collectionName = "Auth_Users";
            var database = client.GetDatabase(databaseName);
            _users = database.GetCollection<AuthUser>(collectionName);
            _logger = logger;

            CreateIndexes();
        }

        private void CreateIndexes()
        {
            var usernameIndex = new CreateIndexModel<AuthUser>(
                Builders<AuthUser>.IndexKeys.Ascending(u => u.Username),
                new CreateIndexOptions { Unique = true }
            );

            var emailIndex = new CreateIndexModel<AuthUser>(
                Builders<AuthUser>.IndexKeys.Ascending(u => u.Email),
                new CreateIndexOptions { Unique = true }
            );

            _users.Indexes.CreateMany(new[] { usernameIndex, emailIndex });
        }

        public async Task<AuthUser?> GetByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<AuthUser?> GetByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<int> GetTokenVersionWithUsername(string username)
        {
            AuthUser user = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
            if(user == null)
            {
                /// throw user not found error
                throw new KeyNotFoundException($"User with username '{username}' not found. Try re-login");
            }
            if(user.TokenVersion == 0) {
                user.TokenVersion = 1;
                var update = Builders<AuthUser>.Update
                    .Set(u => u.TokenVersion, user.TokenVersion);
                await _users.UpdateOneAsync(u => u.Username == username, update);
                _logger.LogInformation("Token version initialized successfully for {Username}", username);
            }
            return user.TokenVersion;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            var count = await _users.CountDocumentsAsync(u => u.Username == username);
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            var count = await _users.CountDocumentsAsync(u => u.Email == email);
            return count > 0;
        }

        public async Task CreateUserAsync(AuthUser user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task<bool> UpdatePasswordAsync(string username, string newPasswordHash)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null)
            {
                _logger.LogWarning("Update password failed: Username {Username} not found", username);
                return false;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Update password failed: Username {Username} is inactive", username);
                return false;
            }

            var update = Builders<AuthUser>.Update
                .Set(u => u.PasswordHash, newPasswordHash)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Username == username, update);

            if (result.ModifiedCount > 0)
            {
                _logger.LogInformation("Password updated successfully for {Username}", username);
                return true;
            }

            _logger.LogWarning("Update password failed: No changes made for {Username}", username);
            return false;
        }

        public async Task<bool> UpdateUsernameAsync(string currentUsername, string newUsername)
        {
            var user = await GetByUsernameAsync(currentUsername);
            if (user == null)
            {
                _logger.LogWarning("Update username failed: Current username {Username} not found", currentUsername);
                return false;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Update username failed: User {Username} is inactive", currentUsername);
                return false;
            }

            if (await UsernameExistsAsync(newUsername))
            {
                _logger.LogWarning("Update username failed: New username {NewUsername} is already taken", newUsername);
                return false;
            }

            var update = Builders<AuthUser>.Update
                .Set(u => u.Username, newUsername)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Username == currentUsername, update);

            if (result.ModifiedCount > 0)
            {
                _logger.LogInformation("Username updated successfully: {CurrentUsername} -> {NewUsername}", currentUsername, newUsername);
                return true;
            }

            _logger.LogWarning("Update username failed: No changes made for {CurrentUsername}", currentUsername);
            return false;
        }

        public async Task<bool> UpdateEmailAsync(EmailChangedEvent emailChangedEvent)
        {
            var user = await GetByUsernameAsync(emailChangedEvent.Username);
            if (user == null)
            {
                _logger.LogWarning("Update email failed: Username {Username} not found", emailChangedEvent.Username);
                return false;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Update email failed: User {Username} is inactive", emailChangedEvent.Username);
                return false;
            }

            if (await EmailExistsAsync(emailChangedEvent.NewEmail))
            {
                _logger.LogWarning("Update email failed: Email {NewEmail} is already taken", emailChangedEvent.NewEmail);
                return false;
            }

            var update = Builders<AuthUser>.Update
                .Set(u => u.Email, emailChangedEvent.NewEmail)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Username == emailChangedEvent.Username, update);

            if (result.ModifiedCount > 0)
            {
                _logger.LogInformation("Email updated successfully: {Username} -> {NewEmail}", emailChangedEvent.Username, emailChangedEvent.NewEmail);
                return true;
            }

            _logger.LogWarning("Update email failed: No changes made for {Username}", emailChangedEvent.Username);
            return false;
        }

        public async Task<bool> UpdateIsActiveStatusAsync(UserDeletedEvent userDeletedEvent)
        {
            var user = await GetByUsernameAsync(userDeletedEvent.Username);
            if (user == null)
            {
                _logger.LogWarning("Deactivate user failed: Username {Username} not found", userDeletedEvent.Username);
                return false;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Deactivate user failed: User {Username} is already inactive", userDeletedEvent.Username);
                return false;
            }

            var update = Builders<AuthUser>.Update
                .Set(u => u.IsActive, false)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _users.UpdateOneAsync(u => u.Username == userDeletedEvent.Username, update);

            if (result.ModifiedCount > 0)
            {
                _logger.LogInformation("User {Username} marked as inactive successfully", userDeletedEvent.Username);
                return true;
            }

            _logger.LogWarning("Deactivate user failed: No changes made for {Username}", userDeletedEvent.Username);
            return false;
        }
    }
}
