using ChatApp.ChatService.Core.DTOs.Chat;
using ChatApp.ChatService.Core.DTOs.Message;
using ChatApp.ChatService.Core.Entities.Message;
using ChatService.Entities.Chat;

namespace ChatApp.ChatService.Core.Mappings
{
    public static class MappingToDtos
    {   
        public static MessageDto MapMessageToDto(Message message)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            return new MessageDto
            {
                MessageId = message.MessageId.ToString(),
                From = message.SenderUsername,
                To = message.ReceiverUsername,
                Time = message.SentAt,
                Text = message.Text,
                MessageType = message.MessageType,
                IsEdited = message.IsEdited,
                MessageStatus = message.MessageStatus.ToString()
            };
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        public static List<MessageDto> MapListOfMessagesToDto(IEnumerable<Message> messages)
        {
            return messages.Select(message => MappingToDtos.MapMessageToDto(message)).ToList();
        }

        public static PrivateChatDto MapPrivateChatToDto(Chat chat)
        {
            // Ensure ParticipantsDetails has at least 2 users
            if (chat.ParticipantsDetails == null || chat.ParticipantsDetails.Count < 2)
            {
                throw new InvalidOperationException("ParticipantsDetails does not contain the required number of participants.");
            }

            List<MessageDto> messages = [];
            foreach (var message in chat.Messages)
            {
                messages.Add(MapMessageToDto(message));
            }

            return new PrivateChatDto
            {
                ChatId = chat.ChatId.ToString(),
                Messages = messages,
                Username1 = chat.ParticipantsDetails[0].Username, // Access first participant
                Username2 = chat.ParticipantsDetails[1].Username, // Access second participant
                CreatedAt = chat.CreatedAt,
                ChatStatus = chat.ChatStatus
            };
        }

    }
}
