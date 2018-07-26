using System;
using System.Runtime.Serialization;

namespace SchedulerBot.Data.Exceptions
{
    public class CalendarNotFoundException : Exception
    {
        public CalendarNotFoundException() { }
        public CalendarNotFoundException(string message) : base(message) { }
        public CalendarNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected CalendarNotFoundException(SerializationInfo info, StreamingContext context) { }
    }
}
