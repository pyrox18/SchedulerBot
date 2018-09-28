using System;
using System.Runtime.Serialization;

namespace SchedulerBot.Client.Exceptions
{
    public class EventParseException : Exception
    {
        public EventParseException() { }
        public EventParseException(string message) : base(message) { }
        public EventParseException(string message, Exception inner) : base(message, inner) { }
        protected EventParseException(SerializationInfo info, StreamingContext context) { }
    }
}
