using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Settings.Models;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Settings.Queries.GetSetting
{
    public class GetSettingQueryHandler :
        IRequestHandler<GetPrefixSettingQuery, PrefixSettingViewModel>,
        IRequestHandler<GetDefaultChannelSettingQuery, DefaultChannelSettingViewModel>,
        IRequestHandler<GetTimezoneSettingQuery, TimezoneSettingViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;

        public GetSettingQueryHandler(ICalendarRepository calendarRepository)
        {
            _calendarRepository = calendarRepository;
        }

        public async Task<PrefixSettingViewModel> Handle(GetPrefixSettingQuery request, CancellationToken cancellationToken = default)
        {
            var calendar = await GetCalendarAsync(request.CalendarId);
            if (string.IsNullOrEmpty(calendar.Prefix))
            {
                throw new CalendarNotInitialisedException(request.CalendarId);
            }

            return new PrefixSettingViewModel
            {
                Prefix = calendar.Prefix
            };
        }

        public async Task<DefaultChannelSettingViewModel> Handle(GetDefaultChannelSettingQuery request, CancellationToken cancellationToken = default)
        {
            var calendar = await GetCalendarAsync(request.CalendarId);
            if (calendar.DefaultChannel == default)
            {
                throw new CalendarNotInitialisedException(request.CalendarId);
            }

            return new DefaultChannelSettingViewModel
            {
                DefaultChannel = calendar.DefaultChannel
            };
        }

        public async Task<TimezoneSettingViewModel> Handle(GetTimezoneSettingQuery request, CancellationToken cancellationToken = default)
        {
            var calendar = await GetCalendarAsync(request.CalendarId);
            if (string.IsNullOrEmpty(calendar.Timezone))
            {
                throw new CalendarNotInitialisedException(request.CalendarId);
            }

            return new TimezoneSettingViewModel
            {
                Timezone = calendar.Timezone
            };
        }

        private async Task<Calendar> GetCalendarAsync(ulong calendarId)
        {
            var calendar = await _calendarRepository.GetByIdAsync(calendarId);
            if (calendar is null)
            {
                throw new CalendarNotInitialisedException(calendarId);
            }

            return calendar;
        }
    }
}