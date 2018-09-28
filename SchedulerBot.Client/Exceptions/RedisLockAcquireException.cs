using System;
using System.Runtime.Serialization;

namespace SchedulerBot.Client.Exceptions
{
    public class RedisLockAcquireException : Exception
    {
        public RedisLockAcquireException() { }
        public RedisLockAcquireException(string message) : base(message) { }
        public RedisLockAcquireException(string message, Exception inner) : base(message, inner) { }
        protected RedisLockAcquireException(SerializationInfo info, StreamingContext context) { }
    }
}
