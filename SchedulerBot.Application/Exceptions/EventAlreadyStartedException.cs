using System;

namespace SchedulerBot.Application.Exceptions
{
    public class EventAlreadyStartedException : Exception
    {
        public EventAlreadyStartedException(Guid eventId) :
            base($"Event {eventId} has already started")
        {
        }
    }
}
