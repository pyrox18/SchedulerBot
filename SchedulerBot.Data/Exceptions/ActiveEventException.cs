using System;
using System.Runtime.Serialization;

namespace SchedulerBot.Data.Exceptions
{
    public class ActiveEventException : Exception
    {
        public ActiveEventException() { }
        public ActiveEventException(string message) : base(message) { }
        public ActiveEventException(string message, Exception inner) : base(message, inner) { }
        protected ActiveEventException(SerializationInfo info, StreamingContext context) { }
    }
}
