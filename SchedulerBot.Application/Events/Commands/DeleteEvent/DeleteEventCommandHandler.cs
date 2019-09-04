using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Specifications;

namespace SchedulerBot.Application.Events.Commands.DeleteEvent
{
    public class DeleteEventCommandHandler : IRequestHandler<DeleteEventByIndexCommand, EventViewModel>
    {
        private readonly IEventRepository _eventRepository;

        public DeleteEventCommandHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<EventViewModel> Handle(DeleteEventByIndexCommand request, CancellationToken cancellationToken = default)
        {
            var events = await _eventRepository.ListAsync(new CalendarEventSpecification(request.CalendarId));

            if (request.Index >= events.Count)
            {
                throw new EventNotFoundException(request.Index);
            }

            var @event = events[request.Index];
            await _eventRepository.DeleteAsync(@event);

            return EventViewModel.FromEvent(@event);
        }
    }
}