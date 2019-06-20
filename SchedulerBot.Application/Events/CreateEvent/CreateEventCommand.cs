using MediatR;
using SchedulerBot.Application.Events.Models;

namespace SchedulerBot.Application.Events.CreateEvent
{
    public class CreateEventCommand : IRequest<EventViewModel>
    {
        public ulong CalendarId { get; set; }
        public string[] EventArgs { get; set; }
    }
}