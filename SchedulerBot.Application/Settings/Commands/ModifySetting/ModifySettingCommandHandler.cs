using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NodaTime;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Settings.Models;
using SchedulerBot.Application.Specifications;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Settings.Commands.ModifySetting
{
    public class ModifySettingCommandHandler :
        IRequestHandler<ModifyPrefixSettingCommand, PrefixSettingViewModel>,
        IRequestHandler<ModifyDefaultChannelSettingCommand, DefaultChannelSettingViewModel>,
        IRequestHandler<ModifyTimezoneSettingCommand, TimezoneSettingViewModel>
    {
        private readonly ICalendarRepository _calendarRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IDateTimeOffset _dateTimeOffset;

        public ModifySettingCommandHandler(ICalendarRepository calendarRepository, IEventRepository eventRepository, IDateTimeOffset dateTimeOffset)
        {
            _calendarRepository = calendarRepository;
            _eventRepository = eventRepository;
            _dateTimeOffset = dateTimeOffset;
        }

        public async Task<PrefixSettingViewModel> Handle(ModifyPrefixSettingCommand request, CancellationToken cancellationToken = default)
        {
            var calendar = await GetCalendar(request.CalendarId);

            calendar.Prefix = request.NewPrefix;

            await _calendarRepository.UpdateAsync(calendar);

            return new PrefixSettingViewModel
            {
                Prefix = calendar.Prefix
            };
        }

        public async Task<DefaultChannelSettingViewModel> Handle(ModifyDefaultChannelSettingCommand request, CancellationToken cancellationToken = default)
        {
            var calendar = await GetCalendar(request.CalendarId);

            calendar.DefaultChannel = request.NewDefaultChannel;

            await _calendarRepository.UpdateAsync(calendar);

            return new DefaultChannelSettingViewModel
            {
                DefaultChannel = calendar.DefaultChannel
            };
        }

        public async Task<TimezoneSettingViewModel> Handle(ModifyTimezoneSettingCommand request, CancellationToken cancellationToken = default)
        {
            var calendar = await GetCalendar(request.CalendarId);
            var events = await _eventRepository.ListAsync(new CalendarEventSpecification(request.CalendarId));

            var tz = DateTimeZoneProviders.Tzdb[request.NewTimezone];
            var oldTz = DateTimeZoneProviders.Tzdb[calendar.Timezone];
            var earliestEvent = events.OrderBy(e => e.StartTimestamp).FirstOrDefault();
            if (earliestEvent != null)
            {
                Instant instant = Instant.FromDateTimeOffset(earliestEvent.StartTimestamp);
                LocalDateTime dt = new ZonedDateTime(instant, oldTz).LocalDateTime;
                ZonedDateTime zdt = tz.AtStrictly(dt);
                if (zdt.ToInstant().ToDateTimeOffset() < _dateTimeOffset.Now)
                {
                    throw new EventStartInNewTimezonePastException();
                }
            }

            calendar.Timezone = request.NewTimezone;
            await _calendarRepository.UpdateAsync(calendar);

            foreach (var evt in events)
            {
                Instant startInstant = Instant.FromDateTimeOffset(evt.StartTimestamp);
                Instant endInstant = Instant.FromDateTimeOffset(evt.EndTimestamp);
                LocalDateTime startDt = new ZonedDateTime(startInstant, oldTz).LocalDateTime;
                LocalDateTime endDt = new ZonedDateTime(endInstant, oldTz).LocalDateTime;
                ZonedDateTime startZdt = tz.AtStrictly(startDt);
                ZonedDateTime endZdt = tz.AtStrictly(endDt);
                evt.StartTimestamp = startZdt.ToDateTimeOffset();
                evt.EndTimestamp = endZdt.ToDateTimeOffset();

                await _eventRepository.UpdateAsync(evt);
            }

            return new TimezoneSettingViewModel
            {
                Timezone = calendar.Timezone,
                DefaultChannel = calendar.DefaultChannel
            };
        }

        private async Task<Calendar> GetCalendar(ulong calendarId)
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
