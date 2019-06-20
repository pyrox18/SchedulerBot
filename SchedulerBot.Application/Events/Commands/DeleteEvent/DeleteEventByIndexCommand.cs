using MediatR;
using SchedulerBot.Application.Events.Models;

namespace SchedulerBot.Application.Events.Commands.DeleteEvent
{
    public class DeleteEventByIndexCommand : IRequest<EventViewModel>
    {
        public ulong CalendarId { get; set; }
        public int Index { get; set; } // Zero-based
    }
}