using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Calendars.Commands.CreateCalendar
{
    public class CreateCalendarCommandHandler : IRequestHandler<CreateCalendarCommand>
    {
        private readonly ICalendarRepository _calendarRepository;

        public CreateCalendarCommandHandler(ICalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        public async Task<Unit> Handle(CreateCalendarCommand request, CancellationToken cancellationToken = default)
        {
            var calendar = new Calendar
            {
                Id = request.CalendarId,
                Prefix = request.Prefix
            };

            await _calendarRepository.AddAsync(calendar);

            return Unit.Value;
        }
    }
}
