using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Constants
{
    public static class QueueNames
    {
        public const string UserRegisteredQueue = "user.registered.queue";
        public const string UserDeletedQueue = "user.deleted.queue";
        public const string UsernameChangedQueue = "user.usernamechanged.queue";
        public const string EmailChangedQueue = "user.emailchanged.queue";

        // Dead Letter Queues (DLQ)
        public const string UserRegisteredDLQ = "user.registered.queue.dlq";
        public const string UserDeletedDLQ = "user.deleted.queue.dlq";
        public const string UsernameChangedDLQ = "user.usernamechanged.queue.dlq";
        public const string EmailChangedDLQ = "user.emailchanged.queue.dlq";
    }
}
