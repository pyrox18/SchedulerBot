using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Interfaces;

namespace SchedulerBot.Application.Calendars.Commands.DeleteCalendar
{
    public class DeleteCalendarCommandHandler : IRequestHandler<DeleteCalendarCommand>
    {
        private readonly ICalendarRepository _calendarRepository;

        public DeleteCalendarCommandHandler(ICalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        public async Task<Unit> Handle(DeleteCalendarCommand request, CancellationToken cancellationToken = default)
        {
            var calendar = await _calendarRepository.GetByIdAsync(request.CalendarId);
            await _calendarRepository.DeleteAsync(calendar);

            return Unit.Value;
        }
    }
}
