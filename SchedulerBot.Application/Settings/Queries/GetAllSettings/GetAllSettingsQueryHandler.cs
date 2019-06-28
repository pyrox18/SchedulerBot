using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Settings.Models;

namespace SchedulerBot.Application.Settings.Queries.GetAllSettings
{
    public class GetAllSettingsQueryHandler : IRequestHandler<GetAllSettingsQuery, SettingsViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;

        public GetAllSettingsQueryHandler(ICalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        public async Task<SettingsViewModel> Handle(GetAllSettingsQuery request, CancellationToken cancellationToken = default)
        {
            var calendar = await _calendarRepository.GetByIdAsync(request.CalendarId);
            if (calendar is null || string.IsNullOrEmpty(calendar.Timezone))
            {
                throw new CalendarNotInitialisedException(request.CalendarId);
            }

            return SettingsViewModel.FromCalendar(calendar);
        }
    }
}