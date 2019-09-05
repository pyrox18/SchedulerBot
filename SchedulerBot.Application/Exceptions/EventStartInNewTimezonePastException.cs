using System;

namespace SchedulerBot.Application.Exceptions
{
    public class EventStartInNewTimezonePastException : Exception
    {
        public EventStartInNewTimezonePastException()
            : base("Cannot change timezone due to events being in the new timezone's past")
        {
        }
    }
}
