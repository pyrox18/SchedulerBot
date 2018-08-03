using System;
using System.Runtime.Serialization;

namespace SchedulerBot.Data.Exceptions
{
    public class ExistingEventInNewTimezonePastException : Exception
    {
        public ExistingEventInNewTimezonePastException () { }
        public ExistingEventInNewTimezonePastException(string message) : base(message) { }
        public ExistingEventInNewTimezonePastException(string message, Exception inner) : base(message, inner) { }
        protected ExistingEventInNewTimezonePastException(SerializationInfo info, StreamingContext context) { }
    }
}
