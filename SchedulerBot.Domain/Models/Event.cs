using System;
using System.Collections.Generic;

namespace SchedulerBot.Domain.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public Calendar Calendar { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartTimestamp { get; set; }
        public DateTimeOffset EndTimestamp { get; set; }
        public DateTimeOffset? ReminderTimestamp { get; set; }
        public RepeatType Repeat { get; set; }
        public List<EventMention> Mentions { get; set; }
        public List<EventRSVP> RSVPs { get; set; }

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
    }

    public enum RepeatType
    {
        None,
        Daily,
        Weekly,
        Monthly,
        MonthlyWeekday
    }
}
