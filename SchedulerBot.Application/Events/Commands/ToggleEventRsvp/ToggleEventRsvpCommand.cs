using MediatR;
using SchedulerBot.Application.Events.Models;

namespace SchedulerBot.Application.Events.Commands.ToggleEventRsvp
{
    public class ToggleEventRsvpCommand : IRequest<EventRsvpViewModel>
    {
        public ulong CalendarId { get; set; }
        public int Index { get; set; }
        public ulong UserId { get; set; }
    }
}