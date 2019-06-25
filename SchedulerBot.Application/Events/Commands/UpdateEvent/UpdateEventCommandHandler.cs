using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;

namespace SchedulerBot.Application.Events.Commands.UpdateEvent
{
    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, EventViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventParser _eventParser;

        public UpdateEventCommandHandler(ICalendarRepository calendarRepository, IEventParser eventParser, IEventRepository eventRepository)
        {
            _calendarRepository = calendarRepository;
            _eventParser = eventParser;
            _eventRepository = eventRepository;
        }

        public async Task<EventViewModel> Handle(UpdateEventCommand request, CancellationToken cancellationToken = default)
        {
            var calendar = await _calendarRepository.GetByIdAsync(request.CalendarId);
            if (calendar is null)
            {
                throw new CalendarNotInitialisedException(request.CalendarId);
            }

            if (request.EventIndex >= calendar.Events.Count)
            {
                throw new EventNotFoundException(request.EventIndex);
            }

            var @event = calendar.Events[request.EventIndex];
            var updatedEvent = _eventParser.ParseUpdateEvent(@event, request.EventArgs, calendar.Timezone);
            await _eventRepository.UpdateAsync(updatedEvent);

            var viewModel = EventViewModel.FromEvent(updatedEvent);
            return viewModel;
        }
    }
}