using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Specifications;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Events.Commands.CleanPastEvents
{
    public class CleanPastEventsCommandHandler : IRequestHandler<CleanPastEventsCommand>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IDateTimeOffset _dateTimeOffset;

        public CleanPastEventsCommandHandler(IEventRepository eventRepository, IDateTimeOffset dateTimeOffset)
        {
            _eventRepository = eventRepository;
            _dateTimeOffset = dateTimeOffset;
        }

        public async Task<Unit> Handle(CleanPastEventsCommand request, CancellationToken cancellationToken = default)
        {
            var now = _dateTimeOffset.Now;
            var spec = new BeforeTimestampEventSpecification(now);
            var events = await _eventRepository.ListAsync(spec);

            foreach (var evt in events)
            {
                if (evt.Repeat == RepeatType.None)
                {
                    await _eventRepository.DeleteAsync(evt);
                }
                else
                {
                    // While loop used to perform repeated repeats until start is after current time
                    while (evt.StartTimestamp < now)
                    {
                        switch (evt.Repeat)
                        {
                            case RepeatType.Daily:
                                evt.StartTimestamp = evt.StartTimestamp.AddDays(1);
                                evt.EndTimestamp = evt.EndTimestamp.AddDays(1);
                                break;
                            case RepeatType.Weekly:
                                evt.StartTimestamp = evt.StartTimestamp.AddDays(7);
                                evt.EndTimestamp = evt.EndTimestamp.AddDays(7);
                                break;
                            case RepeatType.Monthly:
                                evt.StartTimestamp = evt.StartTimestamp.AddMonths(1);
                                evt.EndTimestamp = evt.EndTimestamp.AddMonths(1);
                                break;
                            case RepeatType.MonthlyWeekday:
                                evt.StartTimestamp = RepeatMonthlyWeekday(evt.StartTimestamp);
                                evt.EndTimestamp = RepeatMonthlyWeekday(evt.EndTimestamp);
                                break;
                        }
                    }

                    await _eventRepository.UpdateAsync(evt);
                }
            }

            return Unit.Value;
        }

        // TODO: Move this to Event entity
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
