using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;

namespace SchedulerBot.Application.Events.Commands.DeleteEvent
{
    public class DeleteEventCommandHandler : IRequestHandler<DeleteEventByIndexCommand, EventViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;
        private readonly IEventRepository _eventRepository;

        public DeleteEventCommandHandler(ICalendarRepository calendarRepository, IEventRepository eventRepository)
        {
            _calendarRepository = calendarRepository;
            _eventRepository = eventRepository;
        }

        public async Task<EventViewModel> Handle(DeleteEventByIndexCommand request, CancellationToken cancellationToken = default)
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
            await _eventRepository.DeleteAsync(@event);

            var viewModel = EventViewModel.FromEvent(@event);
            return viewModel;
        }
    }
}