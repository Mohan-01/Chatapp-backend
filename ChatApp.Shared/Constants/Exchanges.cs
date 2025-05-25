namespace Shared.Constants
{
    public class Exchanges
    {
        public const string ChatMessageExchange = "chat.message.exchange";

        // User-related main exchange
        public const string UserEventsExchange = "user.events.exchange";

        // DLQ for user-related messages
        public const string UserEventsDlqExchange = "user.events.exchange.dlq";
    }
}
