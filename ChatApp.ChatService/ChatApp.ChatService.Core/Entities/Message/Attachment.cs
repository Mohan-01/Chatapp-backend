using ChatApp.ChatService.Core.Enums.Message;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatApp.ChatService.Core.Entities.Message
{
    public class Attachment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId AttachmentId { get; set; }

        required public AttachmentType Type { get; set; }

        required public string Url { get; set; } = string.Empty;

        //public string? FileType { get; set; } = string.Empty;

        //public long? FileSize { get; set; } 
    }
}
