using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Specifications;

namespace SchedulerBot.Application.Events.Queries.GetEvent
{
    public class GetEventQueryHandler : IRequestHandler<GetEventByIndexQuery, EventViewModel>
    {
        private readonly IEventRepository _eventRepository;

        public GetEventQueryHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<EventViewModel> Handle(GetEventByIndexQuery request, CancellationToken cancellationToken = default)
        {
            var events = await _eventRepository.ListAsync(new CalendarEventSpecification(request.CalendarId, true));

            if (request.Index >= events.Count)
            {
                throw new EventNotFoundException(request.Index);
            }

            return EventViewModel.FromEvent(events[request.Index]);
        }
    }
}