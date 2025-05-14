using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Enums.User;

namespace AuthService.Core.Models
{
    public class AuthUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? UserId { get; set; }
        required public string Username { get; set; } = null!;
        required public string Email { get; set; } = null!;
        required public string PasswordHash { get; set; } = null!;
        public List<UserRole> Roles { get; set; } = [UserRole.Member];
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public int TokenVersion { get; set; } = 1; // Increment this when the password is changed
    }
}