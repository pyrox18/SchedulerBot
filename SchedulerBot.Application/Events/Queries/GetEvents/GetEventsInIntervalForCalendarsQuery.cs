using MediatR;
using SchedulerBot.Application.Events.Models;
using System;
using System.Collections.Generic;

namespace SchedulerBot.Application.Events.Queries.GetEvents
{
    public class GetEventsInIntervalForCalendarsQuery : IRequest<List<EventWithDefaultChannelViewModel>>
    {
        public IEnumerable<ulong> CalendarIds { get; set; }
        public TimeSpan Interval { get; set; }
    }
}
