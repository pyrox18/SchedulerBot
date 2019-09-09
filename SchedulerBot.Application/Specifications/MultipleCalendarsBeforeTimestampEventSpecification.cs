using SchedulerBot.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchedulerBot.Application.Specifications
{
    public class MultipleCalendarsBeforeTimestampEventSpecification : BaseSpecification<Event>
    {
        public MultipleCalendarsBeforeTimestampEventSpecification(IEnumerable<ulong> calendarIds, DateTimeOffset timestamp)
            : base(e => calendarIds.Contains(e.Calendar.Id) && (e.StartTimestamp <= timestamp || (e.ReminderTimestamp == null) ? false : e.ReminderTimestamp <= timestamp))
        {
            AddInclude(e => e.Calendar);
            AddInclude(e => e.Mentions);
            AddInclude(e => e.RSVPs);
        }
    }
}
