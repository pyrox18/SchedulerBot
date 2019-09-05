using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Specifications;

namespace SchedulerBot.Application.Events.Queries.GetEvents
{
    public class GetEventsQueryHandler : IRequestHandler<GetEventsForCalendarQuery, List<EventViewModel>>
    {
        private readonly IEventRepository _eventRepository;

        public GetEventsQueryHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<List<EventViewModel>> Handle(GetEventsForCalendarQuery request, CancellationToken cancellationToken = default)
        {
            var events = await _eventRepository.ListAsync(new CalendarEventSpecification(request.CalendarId, true));

            return events.Select(EventViewModel.FromEvent).ToList();
        }
    }
}