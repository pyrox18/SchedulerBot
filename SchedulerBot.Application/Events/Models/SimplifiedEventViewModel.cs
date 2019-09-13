using System;
using SchedulerBot.Domain.Models;

namespace SchedulerBot.Application.Events.Models
{
    public class SimplifiedEventViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset StartTimestamp { get; set; }
        public DateTimeOffset EndTimestamp { get; set; }

        public static SimplifiedEventViewModel FromEvent(Event @event)
        {
            return new SimplifiedEventViewModel
            {
                Id = @event.Id,
                Name = @event.Name,
                StartTimestamp = @event.StartTimestamp,
                EndTimestamp = @event.EndTimestamp
            };
        }
    }
}