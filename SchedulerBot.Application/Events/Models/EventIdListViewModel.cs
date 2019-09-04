using System;
using System.Collections.Generic;
using System.Linq;

namespace SchedulerBot.Application.Events.Models
{
    public class EventIdListViewModel
    {
        public List<Guid> EventIds { get; set; }

        public EventIdListViewModel(IEnumerable<Guid> eventIds)
        {
            EventIds = eventIds.ToList();
        }
    }
}
