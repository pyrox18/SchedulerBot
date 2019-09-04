using System;

namespace SchedulerBot.Application.Interfaces
{
    public interface IDateTimeOffset
    {
        DateTimeOffset Now { get; }
    }
}
