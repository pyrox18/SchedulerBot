using SchedulerBot.Domain.Enumerations;
using System;
using System.Collections.Generic;

namespace SchedulerBot.Domain.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public Calendar Calendar { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartTimestamp { get; set; }
        public DateTimeOffset EndTimestamp { get; set; }
        public DateTimeOffset? ReminderTimestamp { get; set; }
        public RepeatType Repeat { get; set; }
        public List<EventMention> Mentions { get; set; }
        public List<EventRSVP> RSVPs { get; set; }

        public bool StartsInHours(double hours)
        {
            return StartTimestamp <= DateTimeOffset.Now.AddHours(hours);
        }

        public bool RemindInHours(double hours)
        {
            if (ReminderTimestamp == null)
            {
                return false;
            }

            return ReminderTimestamp <= DateTimeOffset.Now.AddHours(hours);
        }

        public bool HasStarted()
        {
            return StartTimestamp <= DateTimeOffset.Now;
        }

        public bool HasEnded()
        {
            return EndTimestamp <= DateTimeOffset.Now;
        }

        public bool HasReminderPassed()
        {
            if (ReminderTimestamp == null)
            {
                return true;
            }
            return ReminderTimestamp <= DateTimeOffset.Now;
        }

        public void ApplyRepeat()
        {
            TimeSpan? reminderDifference = null;
            if (!(ReminderTimestamp is null))
            {
                reminderDifference = ((DateTimeOffset)ReminderTimestamp).Subtract(StartTimestamp);
            }

            switch (Repeat)
            {
                case RepeatType.Daily:
                    StartTimestamp = StartTimestamp.AddDays(1);
                    EndTimestamp = EndTimestamp.AddDays(1);
                    break;
                case RepeatType.Weekly:
                    StartTimestamp = StartTimestamp.AddDays(7);
                    EndTimestamp = EndTimestamp.AddDays(7);
                    break;
                case RepeatType.Monthly:
                    StartTimestamp = StartTimestamp.AddMonths(1);
                    EndTimestamp = EndTimestamp.AddMonths(1);
                    break;
                case RepeatType.MonthlyWeekday:
                    StartTimestamp = RepeatMonthlyWeekday(StartTimestamp);
                    EndTimestamp = RepeatMonthlyWeekday(EndTimestamp);
                    break;
            }

            if (!(reminderDifference is null))
            {
                ReminderTimestamp = StartTimestamp.Add((TimeSpan)reminderDifference);
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
