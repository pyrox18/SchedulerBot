using MediatR;
using SchedulerBot.Application.Events.Models;

namespace SchedulerBot.Application.Events.Queries.GetEvent
{
    public class GetEventByIndexQuery : IRequest<EventViewModel>
    {
        public ulong CalendarId { get; set; }
        public int Index { get; set; } // Zero-based
    }
}