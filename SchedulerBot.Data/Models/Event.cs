using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerBot.Data.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartTimestamp { get; set; }
        public DateTimeOffset EndTimestamp { get; set; }
        public RepeatType Repeat { get; set; }
    }

    public enum RepeatType
    {
        None,
        Daily,
        Weekly,
        Monthly
    }
}
