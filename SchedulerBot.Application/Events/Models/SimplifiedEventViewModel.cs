using System;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Events.Models
{
    public class SimplifiedEventViewModel
    {
        public string Name { get; set; }
        public DateTimeOffset StartTimestamp { get; set; }
        public DateTimeOffset EndTimestamp { get; set; }

        public static SimplifiedEventViewModel FromEvent(Event @event)
        {
            return new SimplifiedEventViewModel
            {
                Name = @event.Name,
                StartTimestamp = @event.StartTimestamp,
                EndTimestamp = @event.EndTimestamp
            };
        }
    }
}