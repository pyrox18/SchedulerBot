using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Calendars.Models;
using SchedulerBot.Application.Exceptions;
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
            var calendar = await _calendarRepository.GetByIdAsync(request.CalendarId);

            if (calendar is null)
            {
                calendar = new Calendar
                {
                    Id = request.CalendarId,
                    DefaultChannel = request.ChannelId,
                    Prefix = request.Prefix,
                    Timezone = request.Timezone
                };

                var result = await _calendarRepository.AddAsync(calendar);
                return InitialisedCalendarViewModel.FromCalendar(calendar);
            }
            else if (!string.IsNullOrEmpty(calendar.Timezone))
            {
                throw new CalendarAlreadyInitialisedException(calendar.Id);
            }
            else
            {
                calendar.Timezone = request.Timezone;
                await _calendarRepository.UpdateAsync(calendar);
                return InitialisedCalendarViewModel.FromCalendar(calendar);
            }
        }
    }
}