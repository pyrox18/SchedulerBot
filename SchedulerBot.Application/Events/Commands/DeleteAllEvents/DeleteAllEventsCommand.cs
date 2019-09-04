using MediatR;
using SchedulerBot.Application.Events.Models;

namespace SchedulerBot.Application.Events.Commands.DeleteAllEvents
{
    public class DeleteAllEventsCommand : IRequest<EventIdListViewModel>
    {
        public ulong CalendarId { get; set; }
    }
}