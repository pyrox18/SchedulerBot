using System;
using System.Collections.Generic;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Events.Models
{
    public class EventViewModel
    {
        public ulong CalendarId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartTimestamp { get; set; }
        public DateTimeOffset EndTimestamp { get; set; }
        public DateTimeOffset? ReminderTimestamp { get; set; }
        public RepeatType Repeat { get; set; }
        public List<EventMention> Mentions { get; set; }
        public List<EventRSVP> RSVPs { get; set; }

        public static EventViewModel FromEvent(Event @event)
        {
            return new EventViewModel
            {
                CalendarId = @event.Calendar.Id,
                Name = @event.Name,
                Description = @event.Description,
                StartTimestamp = @event.StartTimestamp,
                EndTimestamp = @event.EndTimestamp,
                ReminderTimestamp = @event.ReminderTimestamp,
                Repeat = @event.Repeat,
                Mentions = @event.Mentions,
                RSVPs = @event.RSVPs
            };
        }
    }
}