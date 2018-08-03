using System;
using System.Runtime.Serialization;

namespace SchedulerBot.Data.Exceptions
{
    public class PermissionNodeNotFoundException : Exception
    {
        public PermissionNodeNotFoundException() { }
        public PermissionNodeNotFoundException(string message) : base(message) { }
        public PermissionNodeNotFoundException(string message, Exception inner) : base(message, inner) { }
        protected PermissionNodeNotFoundException(SerializationInfo info, StreamingContext context) { }
    }
}
