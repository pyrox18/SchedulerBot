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
    public class GetEventsQueryHandler :
        IRequestHandler<GetEventsForCalendarQuery, List<EventViewModel>>,
        IRequestHandler<GetEventsInIntervalForCalendarsQuery, List<EventWithDefaultChannelViewModel>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IDateTimeOffset _dateTimeOffset;

        public GetEventsQueryHandler(IEventRepository eventRepository, IDateTimeOffset dateTimeOffset)
        {
            _eventRepository = eventRepository;
            _dateTimeOffset = dateTimeOffset;
        }

        public async Task<List<EventViewModel>> Handle(GetEventsForCalendarQuery request, CancellationToken cancellationToken = default)
        {
            var events = await _eventRepository.ListAsync(new CalendarEventSpecification(request.CalendarId, true));

            return events.Select(EventViewModel.FromEvent).ToList();
        }

        public async Task<List<EventWithDefaultChannelViewModel>> Handle(GetEventsInIntervalForCalendarsQuery request, CancellationToken cancellationToken = default)
        {
            var intervalTimestamp = _dateTimeOffset.Now.Add(request.Interval);
            var spec = new MultipleCalendarsBeforeTimestampEventSpecification(request.CalendarIds, intervalTimestamp);
            var events = await _eventRepository.ListAsync(spec);

            return events.Select(e => new EventWithDefaultChannelViewModel(e)).ToList();
        }
    }
}