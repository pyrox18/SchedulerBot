using SchedulerBot.Application.Interfaces;
using System;

namespace SchedulerBot.Infrastructure
{
    public class MachineDateTimeOffset : IDateTimeOffset
    {
        public DateTimeOffset Now
        {
            get { return DateTimeOffset.Now; }
        }
    }
}
