using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerBot.Data.Models
{
    public class EventMention
    {
        public Guid Id { get; set; }
        public Event Event { get; set; }
        public ulong TargetId { get; set; }
        public MentionType Type { get; set; }
    }

    public enum MentionType
    {
        Role,
        User,
        Everyone
    }
}
