using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ChatApp.UserService.Core.Enums;

namespace ChatApp.UserService.Core.Entities
{
    public class User2
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; } = ObjectId.GenerateNewId().ToString();

        #region User Information
        required public string Username { get; set; } = null!;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        [BsonRequired]
        required public string Email { get; set; } = null!;
        public string Phone { get; set; } = string.Empty;
        required public List<string> Roles { get; set; } = null!;
        public string ProfilePicture { get; set; } = string.Empty;
        #endregion

        #region Meta Data
        [BsonRepresentation(BsonType.String)]
        public UserStatus Status { get; set; } = UserStatus.Offline;  // Default to offline

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;  // Default active
        #endregion

        #region Security
        public string? PasswordHash { get; set; }  // Nullable for OAuth users
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        #endregion

        #region OAuth
        public string? GoogleId { get; set; }  // Unique Google user ID (OAuth users only)
        public string LoginProvider { get; set; } = "Local";  // "Google" for OAuth users
        #endregion
    }
}
