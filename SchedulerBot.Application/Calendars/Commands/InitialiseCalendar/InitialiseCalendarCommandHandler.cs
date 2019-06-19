using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Calendars.Models;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Calendars.Commands.InitialiseCalendar
{
    public class InitialiseCalendarCommandHandler : IRequestHandler<InitialiseCalendarCommand, InitialisedCalendarViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;

        public InitialiseCalendarCommandHandler(ICalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        public async Task<InitialisedCalendarViewModel> Handle(InitialiseCalendarCommand request,
            CancellationToken cancellationToken = default)
        {
            var calendar = new Calendar
            {
                Id = request.CalendarId,
                DefaultChannel = request.ChannelId,
                Prefix = request.Prefix,
                Timezone = request.Timezone
            };

            var result = await _calendarRepository.AddAsync(calendar);
            var response = new InitialisedCalendarViewModel
            {
                CalendarId = result.Id,
                ChannelId = result.DefaultChannel,
                Timezone = result.Timezone
            };

            return response;
        }
    }
}