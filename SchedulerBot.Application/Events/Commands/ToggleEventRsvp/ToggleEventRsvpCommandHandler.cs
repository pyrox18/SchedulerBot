using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Specifications;
using SchedulerBot.Domain.Models;

namespace SchedulerBot.Application.Events.Commands.ToggleEventRsvp
{
    public class ToggleEventRsvpCommandHandler : IRequestHandler<ToggleEventRsvpCommand, EventRsvpViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IDateTimeOffset _dateTimeOffset;

        public ToggleEventRsvpCommandHandler(ICalendarRepository calendarRepository, IEventRepository eventRepository, IDateTimeOffset dateTimeOffset)
        {
            _calendarRepository = calendarRepository;
            _eventRepository = eventRepository;
            _dateTimeOffset = dateTimeOffset;
        }

        public async Task<EventRsvpViewModel> Handle(ToggleEventRsvpCommand request, CancellationToken cancellationToken = default)
        {
            var calendar = await _calendarRepository.GetByIdAsync(request.CalendarId);
            var events = await _eventRepository.ListAsync(new CalendarEventSpecification(request.CalendarId));

            if (calendar is null)
            {
                throw new CalendarNotInitialisedException(request.CalendarId);
            }

            if (request.Index >= events.Count)
            {
                throw new EventNotFoundException(request.Index);
            }

            var @event = events[request.Index];
            if (@event.StartTimestamp <= _dateTimeOffset.Now)
            {
                throw new EventAlreadyStartedException(@event.Id);
            }

            // TODO: This should be a method in the Event domain model
            var rsvp = @event.RSVPs.FirstOrDefault(r => r.UserId == request.UserId);
            bool rsvpAdded;
            if (rsvp == null)
            {
                @event.RSVPs.Add(new EventRSVP
                {
                    UserId = request.UserId
                });
                rsvpAdded = true;
            }
            else
            {
                @event.RSVPs.Remove(rsvp);
                rsvpAdded = false;
            }

            await _eventRepository.UpdateAsync(@event);
            var viewModel = EventRsvpViewModel.FromEvent(@event);
            viewModel.DefaultChannel = calendar.DefaultChannel;
            viewModel.RsvpAdded = rsvpAdded;
            return viewModel;
        }
    }
}