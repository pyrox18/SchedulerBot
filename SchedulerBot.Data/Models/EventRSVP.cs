using System;

namespace SchedulerBot.Data.Models
{
    public class EventRSVP
    {
        public Guid Id { get; set; }
        public Event Event { get; set; }
        public ulong UserId { get; set; }
    }
}
