using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Shared.Models.User;
using ChatApp.ChatService.Core.Entities.Message;
using ChatApp.ChatService.Core.Enums.Chat;

namespace ChatService.Entities.Chat
{
    public class Chat
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId ChatId { get; set; }

        #region Metadata
        [BsonRepresentation(BsonType.String)]
        public ChatType ChatType { get; set; } = ChatType.Private;
        [BsonRepresentation(BsonType.String)]
        public ChatStatus ChatStatus { get; set; } = ChatStatus.Active;
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? LastMessageTime { get; set; }
        #endregion

        public List<string> Participants { get; set; } = []; // Store Usernames instead of User IDs  
        public List<ObjectId> MessageIds { get; set; } = []; // Store only references

        // Populating values
        public List<UserDto> ParticipantsDetails { get; set; } = [];
        public List<Message> Messages { get; set; } = [];
    }

}