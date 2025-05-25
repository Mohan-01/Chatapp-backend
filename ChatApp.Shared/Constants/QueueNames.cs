namespace Shared.Constants
{
    public static class QueueNames
    {
        public const string UserRegisteredQueue = "user.registered.queue";
        public const string UserDeletedQueue = "user.deleted.queue";
        public const string UsernameChangedQueue = "user.usernamechanged.queue";
        public const string EmailChangedQueue = "user.emailchanged.queue";

        public const string SendMessageQueue = "chat.message.sent";

        // Dead Letter Queues (DLQ)
        public const string UserRegisteredDlqQueue = "user.registered.queue.dlq";
        public const string UserDeletedDlqQueue = "user.deleted.queue.dlq";
        public const string UsernameChangedDlqQueue = "user.usernamechanged.queue.dlq";
        public const string EmailChangedDlqQueue = "user.emailchanged.queue.dlq";

        public const string SendMessageDlqQueue = "chat.message.sent.dlq";
    }
}
