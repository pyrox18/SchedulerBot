using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Specifications;

namespace SchedulerBot.Application.Events.Commands.DeleteAllEvents
{
    public class DeleteAllEventsCommandHandler : IRequestHandler<DeleteAllEventsCommand, EventIdListViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;
        private readonly IEventRepository _eventRepository;

        public DeleteAllEventsCommandHandler(ICalendarRepository calendarRepository, IEventRepository eventRepository)
        {
            _calendarRepository = calendarRepository;
            _eventRepository = eventRepository;
        }

        public async Task<EventIdListViewModel> Handle(DeleteAllEventsCommand request, CancellationToken cancellationToken = default)
        {
            var events = await _eventRepository.ListAsync(new CalendarEventSpecification(request.CalendarId));

            await _calendarRepository.DeleteAllEventsAsync(request.CalendarId);

            return new EventIdListViewModel(events.Select(e => e.Id));
        }
    }
}