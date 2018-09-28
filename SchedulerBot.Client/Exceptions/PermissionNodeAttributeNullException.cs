using System;
using System.Runtime.Serialization;

namespace SchedulerBot.Client.Exceptions
{
    public class PermissionNodeAttributeNullException : Exception
    {
        public PermissionNodeAttributeNullException() { }
        public PermissionNodeAttributeNullException(string message) : base(message) { }
        public PermissionNodeAttributeNullException(string message, Exception inner) : base(message, inner) { }
        protected PermissionNodeAttributeNullException(SerializationInfo info, StreamingContext context) { }
    }
}
