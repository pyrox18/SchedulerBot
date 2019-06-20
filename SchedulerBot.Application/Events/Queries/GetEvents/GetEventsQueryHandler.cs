using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;

namespace SchedulerBot.Application.Events.Queries.GetEvents
{
    public class GetEventsQueryHandler : IRequestHandler<GetEventsForCalendarQuery, List<SimplifiedEventViewModel>>
    {
        private readonly ICalendarRepository _calendarRepository;

        public GetEventsQueryHandler(ICalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        public async Task<List<SimplifiedEventViewModel>> Handle(GetEventsForCalendarQuery request, CancellationToken cancellationToken = default)
        {
            var calendar = await _calendarRepository.GetByIdAsync(request.CalendarId);
            if (calendar is null)
            {
                throw new CalendarNotInitialisedException(request.CalendarId);
            }

            var viewModelList = calendar.Events.Select(SimplifiedEventViewModel.FromEvent).ToList();
            return viewModelList;
        }
    }
}