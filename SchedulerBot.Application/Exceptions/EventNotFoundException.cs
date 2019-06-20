using System;

namespace SchedulerBot.Application.Exceptions
{
    public class EventNotFoundException : Exception
    {
        public EventNotFoundException()
            : base("Event not found")
        {
        }

        public EventNotFoundException(int index)
            : base($"Event with index {index} not found")
        {
        }
    }
}