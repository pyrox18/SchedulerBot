using System;
using System.Runtime.Serialization;

namespace SchedulerBot.Client.Exceptions
{
    public class DateTimeInPastException : Exception
    {
        public DateTimeInPastException() { }
        public DateTimeInPastException(string message) : base(message) { }
        public DateTimeInPastException(string message, Exception inner) : base(message, inner) { }
        protected DateTimeInPastException(SerializationInfo info, StreamingContext context) { }
    }
}
