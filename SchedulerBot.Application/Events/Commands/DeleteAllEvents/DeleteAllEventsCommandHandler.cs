using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Interfaces;

namespace SchedulerBot.Application.Events.Commands.DeleteAllEvents
{
    public class DeleteAllEventsCommandHandler : IRequestHandler<DeleteAllEventsCommand>
    {
        private readonly ICalendarRepository _calendarRepository;

        public DeleteAllEventsCommandHandler(ICalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        public async Task<Unit> Handle(DeleteAllEventsCommand request, CancellationToken cancellationToken = default)
        {
            await _calendarRepository.DeleteAllEventsAsync(request.CalendarId);

            return Unit.Value;
        }
    }
}