using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Events.Commands.ToggleEventRsvp
{
    public class ToggleEventRsvpCommandHandler : IRequestHandler<ToggleEventRsvpCommand, EventWithDefaultChannelViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;
        private readonly IEventRepository _eventRepository;

        public ToggleEventRsvpCommandHandler(ICalendarRepository calendarRepository, IEventRepository eventRepository)
        {
            _calendarRepository = calendarRepository;
            _eventRepository = eventRepository;
        }

        public async Task<EventWithDefaultChannelViewModel> Handle(ToggleEventRsvpCommand request, CancellationToken cancellationToken = default)
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

            var @event = calendar.Events[request.Index]; 
            // TODO: This should be a method in the Event domain model
            var rsvp = @event.RSVPs.FirstOrDefault(r => r.UserId == request.UserId);
            if (rsvp == null)
            {
                @event.RSVPs.Add(new EventRSVP
                {
                    UserId = request.UserId
                });
            }
            else
            {
                @event.RSVPs.Remove(rsvp);
            }

            await _eventRepository.UpdateAsync(@event);
            var viewModel = EventWithDefaultChannelViewModel.FromEvent(@event);
            viewModel.DefaultChannel = calendar.DefaultChannel;
            return viewModel;
        }
    }
}