using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using SchedulerBot.Data.Exceptions;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Data.Services
{
    public class EventService : IEventService
    {
        private readonly SchedulerBotContext _context;

        public EventService(SchedulerBotContext context) => _context = context;

        public async Task<Event> CreateEventAsync(ulong calendarId, Event evt)
        {
            var calendar = await _context.Calendars
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.Id == calendarId);
            if (calendar == null)
            {
                throw new CalendarNotFoundException();
            }

            evt.Id = Guid.NewGuid();
            calendar.Events.Add(evt);
            await _context.SaveChangesAsync();

            return evt;
        }

        public async Task<List<Event>> GetEventsAsync(ulong calendarId)
        {
            var isCalendarExists = await _context.Calendars.AnyAsync(c => c.Id == calendarId);
            if (!isCalendarExists)
            {
                throw new CalendarNotFoundException();
            }

            var events = await _context.Events
                .Include(e => e.Mentions)
                .Where(e => e.Calendar.Id == calendarId)
                .ToListAsync();

            var timezone = await _context.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Timezone)
                .FirstOrDefaultAsync();

            var orderedEvents = events.OrderBy(e => e.StartTimestamp).ToList();

            for (int i = 0; i < orderedEvents.Count; i++)
            {
                Event evt = orderedEvents[i];
                AdjustTimestampsToTimezone(ref evt, timezone);
                orderedEvents[i] = evt;
            }

            return orderedEvents;
        }

        public async Task<Event> DeleteEventAsync(ulong calendarId, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("Index must be greater than 0");
            }

            Event deletedEvent;
            string timezone;

            var isCalendarExists = await _context.Calendars.AnyAsync(c => c.Id == calendarId);
            if (!isCalendarExists)
            {
                throw new CalendarNotFoundException();
            }

            var events = await _context.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Events)
                .FirstOrDefaultAsync();
            events = events.OrderBy(e => e.StartTimestamp).ToList();
            if (index + 1 > events.Count)
            {
                throw new EventNotFoundException();
            }

            deletedEvent = await _context.Events
                .Include(e => e.Mentions)
                .Include(e => e.RSVPs)
                .FirstOrDefaultAsync(e => e.Id == events[index].Id);
            _context.EventMentions.RemoveRange(deletedEvent.Mentions);
            _context.EventRSVPs.RemoveRange(deletedEvent.RSVPs);
            _context.Events.Remove(deletedEvent);
            await _context.SaveChangesAsync();

            timezone = await _context.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Timezone)
                .FirstOrDefaultAsync();

            AdjustTimestampsToTimezone(ref deletedEvent, timezone);

            return deletedEvent;
        }

        public async Task<Event> DeleteEventAsync(Guid eventId)
        {
            var evt = await _context.Events
                .Include(e => e.Mentions)
                .Include(e => e.RSVPs)
                .FirstOrDefaultAsync(e => e.Id == eventId);
            _context.EventMentions.RemoveRange(evt.Mentions);
            _context.EventRSVPs.RemoveRange(evt.RSVPs);
            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();

            return evt;
        }

        public async Task<List<Event>> DeleteAllEventsAsync(ulong calendarId)
        {
            var isCalendarExists = await _context.Calendars.AnyAsync(c => c.Id == calendarId);
            if (!isCalendarExists)
            {
                throw new CalendarNotFoundException();
            }

            var events = await _context.Events
                .Where(e => e.Calendar.Id == calendarId)
                .ToListAsync();

            var mentions = await _context.EventMentions
                .Where(m => m.Event.Calendar.Id == calendarId)
                .ToListAsync();

            var rsvps = await _context.EventRSVPs
                .Where(r => r.Event.Calendar.Id == calendarId)
                .ToListAsync();

            _context.Events.RemoveRange(events);
            _context.EventMentions.RemoveRange(mentions);
            _context.EventRSVPs.RemoveRange(rsvps);
            await _context.SaveChangesAsync();

            return events;
        }

        public async Task<Event> GetEventByIndexAsync(ulong calendarId, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("Index must be greater than 0");
            }

            var isCalendarExists = await _context.Calendars.AnyAsync(c => c.Id == calendarId);
            if (!isCalendarExists)
            {
                throw new CalendarNotFoundException();
            }

            var events = await _context.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Events)
                .FirstOrDefaultAsync();
            events = events.OrderBy(e => e.StartTimestamp).ToList();

            var evt = events[index];
            evt = await _context.Events
                .Include(e => e.Mentions)
                .Include(e => e.RSVPs)
                .Where(e => e == evt)
                .FirstOrDefaultAsync();

            var timezone = await _context.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Timezone)
                .FirstOrDefaultAsync();

            AdjustTimestampsToTimezone(ref evt, timezone);

            return evt;
        }

        public async Task<Event> UpdateEventAsync(Event evt)
        {
            var mentionsToDelete = await _context.EventMentions
                .Where(m => m.Event.Id == evt.Id)
                .ToListAsync();
            _context.EventMentions.RemoveRange(mentionsToDelete);
            await _context.SaveChangesAsync();

            var eventInDb = await _context.Events
                .Include(e => e.Calendar)
                .Include(e => e.Mentions)
                .Include(e => e.RSVPs)
                .FirstOrDefaultAsync(e => e.Id == evt.Id);
            eventInDb.Name = evt.Name;
            eventInDb.StartTimestamp = evt.StartTimestamp;
            eventInDb.EndTimestamp = evt.EndTimestamp;
            eventInDb.ReminderTimestamp = evt.ReminderTimestamp;
            eventInDb.Description = evt.Description;
            eventInDb.Repeat = evt.Repeat;
            eventInDb.Mentions = evt.Mentions;

            await _context.SaveChangesAsync();

            var timezone = await _context.Calendars
                .Where(c => c.Id == eventInDb.Calendar.Id)
                .Select(c => c.Timezone)
                .FirstOrDefaultAsync();

            AdjustTimestampsToTimezone(ref eventInDb, timezone);

            return eventInDb;
        }

        public async Task<Event> ApplyRepeatAsync(Guid eventId)
        {
            var evt = await _context.Events
                .Include(e => e.Calendar)
                .Include(e => e.Mentions)
                .Include(e => e.RSVPs)
                .FirstOrDefaultAsync(e => e.Id == eventId);
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
                case RepeatType.None:
                default:
                    break;
            }

            await _context.SaveChangesAsync();

            AdjustTimestampsToTimezone(ref evt, evt.Calendar.Timezone);

            return evt;
        }

        public async Task<List<Event>> GetEventsInHourIntervalAsync(double hours)
        {
            var events = await _context.Events
                .Include(e => e.Calendar)
                .Include(e => e.Mentions)
                .Include(e => e.RSVPs)
                .Where(e => e.StartsInHours(hours) || e.RemindInHours(hours))
                .ToListAsync();

            for (int i = 0; i < events.Count; i++)
            {
                Event evt = events[i];
                AdjustTimestampsToTimezone(ref evt, evt.Calendar.Timezone);
                events[i] = evt;
            }

            return events;
        }

        public async Task<List<Event>> GetEventsInHourIntervalAsync(double hours, IEnumerable<ulong> guildIds)
        {
            var events = await _context.Events
                .Include(e => e.Calendar)
                .Include(e => e.Mentions)
                .Include(e => e.RSVPs)
                .Where(e => guildIds.Contains(e.Calendar.Id) && (e.StartsInHours(hours) || e.RemindInHours(hours)))
                .ToListAsync();

            for (int i = 0; i < events.Count; i++)
            {
                Event evt = events[i];
                AdjustTimestampsToTimezone(ref evt, evt.Calendar.Timezone);
                events[i] = evt;
            }

            return events;
        }

        public async Task<Event> ToggleRSVPByIndexAsync(ulong calendarId, ulong userId, int index)
        {
            var isCalendarExists = await _context.Calendars.AnyAsync(c => c.Id == calendarId);
            if (!isCalendarExists)
            {
                throw new CalendarNotFoundException();
            }

            var events = await _context.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Events)
                .FirstOrDefaultAsync();
            events = events.OrderBy(e => e.StartTimestamp).ToList();

            var evt = events[index];
            evt = await _context.Events
                .Include(e => e.Mentions)
                .Include(e => e.RSVPs)
                .Where(e => e == evt)
                .FirstOrDefaultAsync();

            if (evt.StartTimestamp <= DateTimeOffset.Now)
            {
                throw new ActiveEventException();
            }

            var rsvp = evt.RSVPs.FirstOrDefault(r => r.UserId == userId);
            if (rsvp == null)
            {
                evt.RSVPs.Add(new EventRSVP
                {
                    UserId = userId
                });
            }
            else
            {
                evt.RSVPs.Remove(rsvp);
                _context.EventRSVPs.Remove(rsvp);
            }

            await _context.SaveChangesAsync();

            var timezone = await _context.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Timezone)
                .FirstOrDefaultAsync();

            AdjustTimestampsToTimezone(ref evt, timezone);

            return evt;
        }

        public async Task ApplyDeleteAndRepeatPastEventsAsync()
        {
            var events = await _context.Events
                .Include(e => e.Mentions)
                .Include(e => e.RSVPs)
                .Where(e => e.EndTimestamp < DateTimeOffset.Now)
                .ToListAsync();

            foreach (var evt in events)
            {
                if (evt.Repeat == RepeatType.None)
                {
                    _context.EventMentions.RemoveRange(evt.Mentions);
                    _context.EventRSVPs.RemoveRange(evt.RSVPs);
                    _context.Events.Remove(evt);
                }
                else
                {

                    while (evt.StartTimestamp < DateTimeOffset.Now)
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
                            default:
                                break;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        private void AdjustTimestampsToTimezone(ref Event evt, string timezone)
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
