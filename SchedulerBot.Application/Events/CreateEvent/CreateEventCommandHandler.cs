using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;

namespace SchedulerBot.Application.Events.CreateEvent
{
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, EventViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventParser _eventParser;

        public CreateEventCommandHandler(ICalendarRepository calendarRepository, IEventRepository eventRepository, IEventParser eventParser)
        {
            _calendarRepository = calendarRepository;
            _eventRepository = eventRepository;
            _eventParser = eventParser;
        }

        public async Task<EventViewModel> Handle(CreateEventCommand request, CancellationToken cancellationToken = default)
        {
            var calendar = await _calendarRepository.GetByIdAsync(request.CalendarId);
            if (calendar is null || string.IsNullOrEmpty(calendar.Timezone))
            {
                throw new CalendarNotInitialisedException(request.CalendarId);
            }

            var @event = _eventParser.ParseNewEvent(request.EventArgs, calendar.Timezone);
            @event.Calendar = calendar;

            var result = await _eventRepository.AddAsync(@event);

            var viewModel = EventViewModel.FromEvent(result);
            return viewModel;
        }
    }
}