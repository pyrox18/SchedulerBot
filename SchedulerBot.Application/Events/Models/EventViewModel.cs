using System;
using System.Collections.Generic;
using SchedulerBot.Domain.Enumerations;
using SchedulerBot.Domain.Models;

namespace SchedulerBot.Application.Events.Models
{
    public class EventViewModel
    {
        public ulong CalendarId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartTimestamp { get; set; }
        public DateTimeOffset EndTimestamp { get; set; }
        public DateTimeOffset? ReminderTimestamp { get; set; }
        public RepeatType Repeat { get; set; }
        public List<EventMention> Mentions { get; set; }
        public List<EventRSVP> RSVPs { get; set; }

        public EventViewModel()
        {
        }

        public EventViewModel(Event @event)
        {
            CalendarId = @event.Calendar.Id;
            Id = @event.Id;
            Name = @event.Name;
            Description = @event.Description;
            StartTimestamp = @event.StartTimestamp;
            EndTimestamp = @event.EndTimestamp;
            ReminderTimestamp = @event.ReminderTimestamp;
            Repeat = @event.Repeat;
            Mentions = @event.Mentions;
            RSVPs = @event.RSVPs;
        }

        public bool StartsInHours(double hours)
        {
            return StartTimestamp <= DateTimeOffset.Now.AddHours(hours);
        }

        public bool RemindInHours(double hours)
        {
            if (ReminderTimestamp == null)
            {
                return false;
            }

            return ReminderTimestamp <= DateTimeOffset.Now.AddHours(hours);
        }

        public bool HasStarted()
        {
            return StartTimestamp <= DateTimeOffset.Now;
        }

        public bool HasEnded()
        {
            return EndTimestamp <= DateTimeOffset.Now;
        }

        public bool HasReminderPassed()
        {
            if (ReminderTimestamp == null)
            {
                return true;
            }
            return ReminderTimestamp <= DateTimeOffset.Now;
        }

        public static EventViewModel FromEvent(Event @event)
        {
            return new EventViewModel(@event);
        }
    }
}