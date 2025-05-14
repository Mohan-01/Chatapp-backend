using ChatApp.ChatService.Core.Enums.Message;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatApp.ChatService.Core.Entities.Message
{
    public class Message
    {
        [BsonId]
        public string MessageId { get; set; } = string.Empty;

        // Every message should be part of a chat
        [BsonRepresentation(BsonType.ObjectId)]
        required public ObjectId ChatId { get; set; }

        #region Message Info
        required public string SenderUsername { get; set; } = null!;
        public string? ReceiverUsername { get; set; }  // Null for group messages  
        public string Text { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.String)]
        required public MessageType MessageType { get; set; } = MessageType.Text;
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public List<Attachment> Attachments { get; set; } = [];
        #endregion

        #region Metadata
        public bool IsEdited { get; set; } = false;

        [BsonRepresentation(BsonType.String)]
        public MessageStatus MessageStatus { get; set; } = MessageStatus.Sent;
        #endregion
    }
}
