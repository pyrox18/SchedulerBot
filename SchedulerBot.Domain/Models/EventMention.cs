using SchedulerBot.Domain.Enumerations;
using System;

namespace SchedulerBot.Domain.Models
{
    public class EventMention
    {
        public Guid Id { get; set; }
        public Event Event { get; set; }
        public ulong TargetId { get; set; }
        public MentionType Type { get; set; }
    }
}
