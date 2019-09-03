using System;

namespace SchedulerBot.Application.Exceptions
{
    public class CalendarAlreadyInitialisedException : Exception
    {
        public CalendarAlreadyInitialisedException(ulong id)
            : base($"Calendar with ID {id} already initialised")
        {
        }
    }
}