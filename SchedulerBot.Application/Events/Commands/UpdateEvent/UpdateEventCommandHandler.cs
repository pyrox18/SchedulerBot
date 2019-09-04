using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Specifications;

namespace SchedulerBot.Application.Events.Commands.UpdateEvent
{
    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, EventViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEventParser _eventParser;
        private readonly IDateTimeOffset _dateTimeOffset;

        public UpdateEventCommandHandler(ICalendarRepository calendarRepository, IEventParser eventParser, IEventRepository eventRepository, IDateTimeOffset dateTimeOffset)
        {
            _calendarRepository = calendarRepository;
            _eventParser = eventParser;
            _eventRepository = eventRepository;
            _dateTimeOffset = dateTimeOffset;
        }

        public async Task<EventViewModel> Handle(UpdateEventCommand request, CancellationToken cancellationToken = default)
        {
            var calendar = await _calendarRepository.GetByIdAsync(request.CalendarId);
            if (calendar is null)
            {
                throw new CalendarNotInitialisedException(request.CalendarId);
            }

            var events = await _eventRepository.ListAsync(new CalendarEventSpecification(request.CalendarId));

            if (request.EventIndex >= events.Count)
            {
                throw new EventNotFoundException(request.EventIndex);
            }

            var @event = events[request.EventIndex];
            if (@event.StartTimestamp <= _dateTimeOffset.Now)
            {
                throw new EventAlreadyStartedException(@event.Id);
            }

            var updatedEvent = _eventParser.ParseUpdateEvent(@event, request.EventArgs, calendar.Timezone);
            await _eventRepository.UpdateAsync(updatedEvent);

            return EventViewModel.FromEvent(updatedEvent);
        }
    }
}