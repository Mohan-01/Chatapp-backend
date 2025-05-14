using ChatApp.ChatService.Core.Enums.Chat;
using MongoDB.Bson;

public class Pipelines
{
    public static BsonDocument[] ChatsWithUserDetails(string chatId) =>
    [
        // Match chat by ID
        new BsonDocument("$match", new BsonDocument("_id", new ObjectId(chatId))),

        // Lookup participants from Users collection by Username
        //new BsonDocument("$lookup", new BsonDocument
        //{
        //    { "from", "Users" },
        //    { "localField", "Participants" },  // Match Participants array (which are usernames)
        //    { "foreignField", "Username" },    // Compare against Users.Username
        //    { "as", "ParticipantsDetails" }    // Store matched users in ParticipantsDetails
        //}),

        // Lookup messages from Messages collection
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "Messages" },
            { "localField", "_id" },
            { "foreignField", "ChatId" },
            { "as", "Messages" }
        })
    ];


    public static BsonDocument[] ChatsWithUserDetails(string username, ChatStatus chatStatus) =>
    [
        // Match chats where the user is a participant
        new BsonDocument("$match", new BsonDocument
        {
            { "Participants", new BsonDocument("$in", new BsonArray { new BsonString(username) }) }, // Ensure username is in Participants array
            { "ChatStatus", new BsonString(chatStatus.ToString()) }
        }),

        // Lookup participant details from Users by Username
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "Users" },
            { "localField", "Participants" },
            { "foreignField", "Username" },  // Match by `Username`
            { "as", "ParticipantsDetails" }
        }),

        // Lookup messages
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "Messages" },
            { "localField", "_id" },
            { "foreignField", "ChatId" },
            { "as", "Messages" }
        }),
    ];


    public static BsonDocument[] ChatsWithUserDetails(string username1, string username2) =>
    [
        // Match chat where both users are participants
        new BsonDocument("$match", new BsonDocument
        {
            { "ChatType", ChatType.Private.ToString() },
            { "Participants", new BsonDocument("$all", new BsonArray { new BsonString(username1), new BsonString(username2) }) } // Ensure type consistency
        }),


        // Lookup participants
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "Users" },
            { "localField", "Participants" },
            { "foreignField", "Username" },  // Match by `Username`
            { "as", "ParticipantsDetails" }
        }),

        // Lookup messages
        new BsonDocument("$lookup", new BsonDocument
        {
            { "from", "Messages" },
            { "localField", "_id" },
            { "foreignField", "ChatId" },
            { "as", "Messages" }
        }),
    ];

}
