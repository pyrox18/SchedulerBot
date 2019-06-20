using MediatR;

namespace SchedulerBot.Application.Events.Commands.DeleteAllEvents
{
    public class DeleteAllEventsCommand : IRequest
    {
        public ulong CalendarId { get; set; }
    }
}