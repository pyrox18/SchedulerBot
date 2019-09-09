using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NodaTime;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Events.Commands.ApplyEventRepeat
{
    public class ApplyEventRepeatCommandHandler : IRequestHandler<ApplyEventRepeatCommand, EventViewModel>
    {
        private readonly IEventRepository _eventRepository;

        public ApplyEventRepeatCommandHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<EventViewModel> Handle(ApplyEventRepeatCommand request, CancellationToken cancellationToken = default)
        {
            var @event = await _eventRepository.GetByIdAsync(request.EventId);

            TimeSpan? reminderDifference = null;
            if (!(@event.ReminderTimestamp is null))
            {
                reminderDifference = ((DateTimeOffset)@event.ReminderTimestamp).Subtract(@event.StartTimestamp);
            }

            switch (@event.Repeat)
            {
                case RepeatType.Daily:
                    @event.StartTimestamp = @event.StartTimestamp.AddDays(1);
                    @event.EndTimestamp = @event.EndTimestamp.AddDays(1);
                    break;
                case RepeatType.Weekly:
                    @event.StartTimestamp = @event.StartTimestamp.AddDays(7);
                    @event.EndTimestamp = @event.EndTimestamp.AddDays(7);
                    break;
                case RepeatType.Monthly:
                    @event.StartTimestamp = @event.StartTimestamp.AddMonths(1);
                    @event.EndTimestamp = @event.EndTimestamp.AddMonths(1);
                    break;
                case RepeatType.MonthlyWeekday:
                    @event.StartTimestamp = RepeatMonthlyWeekday(@event.StartTimestamp);
                    @event.EndTimestamp = RepeatMonthlyWeekday(@event.EndTimestamp);
                    break;
            }

            if (!(reminderDifference is null))
            {
                @event.ReminderTimestamp = @event.StartTimestamp.Add((TimeSpan)reminderDifference);
            }

            await _eventRepository.UpdateAsync(@event);

            AdjustTimestampsToTimezone(@event, @event.Calendar.Timezone);

            return EventViewModel.FromEvent(@event);
        }

        private void AdjustTimestampsToTimezone(Event evt, string timezone)
        {
            var tz = DateTimeZoneProviders.Tzdb[timezone];
            Instant instant = Instant.FromDateTimeOffset(evt.StartTimestamp);
            LocalDateTime dt = new ZonedDateTime(instant, DateTimeZoneProviders.Tzdb[timezone]).LocalDateTime;
            ZonedDateTime zdt = tz.AtLeniently(dt);
            evt.StartTimestamp = zdt.ToDateTimeOffset();

            instant = Instant.FromDateTimeOffset(evt.EndTimestamp);
            dt = new ZonedDateTime(instant, DateTimeZoneProviders.Tzdb[timezone]).LocalDateTime;
            zdt = tz.AtLeniently(dt);
            evt.EndTimestamp = zdt.ToDateTimeOffset();

            if (evt.ReminderTimestamp != null)
            {
                instant = Instant.FromDateTimeOffset((DateTimeOffset)evt.ReminderTimestamp);
                dt = new ZonedDateTime(instant, DateTimeZoneProviders.Tzdb[timezone]).LocalDateTime;
                zdt = tz.AtLeniently(dt);
                evt.ReminderTimestamp = zdt.ToDateTimeOffset();
            }
        }

        private DateTimeOffset RepeatMonthlyWeekday(DateTimeOffset dt)
        {
            int currentMonth = dt.Month;
            int nextMonth = dt.AddMonths(1).Month;
            int weekdayIndex = 0;
            DateTimeOffset m = dt;
            List<DateTimeOffset> monthList = new List<DateTimeOffset>();
            List<DateTimeOffset> nextMonthList = new List<DateTimeOffset>();

            // Generate all the 1st, 2nd, 3rd, etc weekday information for the current month
            monthList.Add(m);
            do
            {  // Go back one week at a time until we hit the previous month
                m = m.AddDays(-7);
                if (m.Month == currentMonth)
                {
                    monthList.Insert(0, m);
                    weekdayIndex++;  // eg. the nth Monday of the month
                }
            } while (m.Month == currentMonth);

            m = dt;
            do
            {  // Go forward one week at a time until we hit the next month
                m = m.AddDays(7);
                if (m.Month == currentMonth)
                {
                    monthList.Add(m);
                }
            } while (m.Month == currentMonth);

            // Do the same thing for the month after
            nextMonthList.Add(m);
            do
            {
                m = m.AddDays(7);
                if (m.Month == nextMonth)
                {
                    nextMonthList.Add(m);
                }
            } while (m.Month == nextMonth);

            // monthList     = [m-7, m-7,   m, m+7, m+7, m+7]
            // nextMonthList = [  n, n+7, n+7, n+7, n+7]

            if (weekdayIndex < nextMonthList.Count)
            {
                return nextMonthList[weekdayIndex];
            }
            else
            {  // eg. last Monday of the month
                return nextMonthList[nextMonthList.Count - 1];
            }
        }
    }
}
