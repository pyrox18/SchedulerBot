using System;
using System.Runtime.Serialization;

namespace SchedulerBot.Client.Exceptions
{
    public class EventEndBeforeStartException : Exception
    {
        public EventEndBeforeStartException () { }
        public EventEndBeforeStartException (string message) : base(message) { }
        public EventEndBeforeStartException (string message, Exception inner) : base(message, inner) { }
        protected EventEndBeforeStartException(SerializationInfo info, StreamingContext context) { }
    }
}
