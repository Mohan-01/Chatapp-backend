using ChatApp.ChatService.Core.Enums.Friend;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatApp.ChatService.Core.Entities.Friendship
{
    public class Friendship
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string SenderUsername { get; set; }  // The user who sent the friend request

        public string RecipientUsername { get; set; }  // The user receiving the request

        [BsonRepresentation(BsonType.String)]
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.NA;

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime SentAt { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime? RespondedAt { get; set; }
    }

}
