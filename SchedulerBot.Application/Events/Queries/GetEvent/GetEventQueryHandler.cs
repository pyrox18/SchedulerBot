using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;

namespace SchedulerBot.Application.Events.Queries.GetEvent
{
    public class GetEventQueryHandler : IRequestHandler<GetEventByIndexQuery, EventViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;

        public GetEventQueryHandler(ICalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        public async Task<EventViewModel> Handle(GetEventByIndexQuery request, CancellationToken cancellationToken = default)
        {
            var calendar = await _calendarRepository.GetByIdAsync(request.CalendarId);
            if (calendar is null)
            {
                throw new CalendarNotInitialisedException(request.CalendarId);
            }

            if (request.Index >= calendar.Events.Count)
            {
                throw new EventNotFoundException(request.Index);
            }

            var viewModel = EventViewModel.FromEvent(calendar.Events[request.Index]);
            return viewModel;
        }
    }
}