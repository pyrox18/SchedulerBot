using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Settings.Models;

namespace SchedulerBot.Application.Settings.Queries.GetSetting
{
    public class GetSettingQueryHandler
        : IRequestHandler<GetPrefixSettingQuery, PrefixSettingViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;

        public GetSettingQueryHandler(ICalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        public async Task<PrefixSettingViewModel> Handle(GetPrefixSettingQuery request, CancellationToken cancellationToken = default)
        {
            var calendar = await _calendarRepository.GetByIdAsync(request.CalendarId);
            if (calendar is null || string.IsNullOrEmpty(calendar.Prefix))
            {
                throw new CalendarNotInitialisedException(request.CalendarId);
            }

            return new PrefixSettingViewModel
            {
                Prefix = calendar.Prefix
            };
        }
    }
}