using System;

namespace SchedulerBot.Application.Exceptions
{
    public class CalendarNotInitialisedException : Exception
    {
        public CalendarNotInitialisedException(ulong id)
            : base($"Calendar with ID {id} not initialised")
        {
        }
    }
}