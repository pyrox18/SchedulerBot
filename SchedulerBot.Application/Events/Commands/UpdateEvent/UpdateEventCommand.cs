using MediatR;
using SchedulerBot.Application.Events.Models;

namespace SchedulerBot.Application.Events.Commands.UpdateEvent
{
    public class UpdateEventCommand : IRequest<EventViewModel>
    {
        public ulong CalendarId { get; set; }
        public int EventIndex { get; set; } // Zero-based
        public string[] EventArgs { get; set; }
    }
}