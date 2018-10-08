using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NodaTime;
using SchedulerBot.Data.Exceptions;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Data.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly SchedulerBotContext _context;
        private readonly string _defaultPrefix;

        public CalendarService(SchedulerBotContext context, IConfigurationRoot configuration)
        {
            _context = context;
            _defaultPrefix = configuration.GetSection("Bot").GetSection("Prefixes").Get<string[]>()[0];
        }

        public async Task<Calendar> CreateCalendarAsync(Calendar calendar)
        {
            await _context.Calendars.AddAsync(calendar);
            await _context.SaveChangesAsync();

            return calendar;
        }

        public async Task<bool> DeleteCalendarAsync(ulong calendarId)
        {
            var permissionsToRemove = await _context.Permissions.Where(p => p.Calendar.Id == calendarId).ToListAsync();
            var calendarToRemove = await _context.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);

            _context.Permissions.RemoveRange(permissionsToRemove);
            _context.Calendars.Remove(calendarToRemove);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> GetCalendarPrefixAsync(ulong calendarId)
        {
            return await _context.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Prefix)
                .FirstOrDefaultAsync();
        }

        public async Task<string> UpdateCalendarPrefixAsync(ulong calendarId, string newPrefix)
        {
            var calendar = await _context.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
            if (calendar == null)
            {
                throw new CalendarNotFoundException();
            }

            calendar.Prefix = newPrefix;
            await _context.SaveChangesAsync();

            return calendar.Prefix;
        }

        public async Task<ulong> GetCalendarDefaultChannelAsync(ulong calendarId)
        {
            return await _context.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.DefaultChannel)
                .FirstOrDefaultAsync();
        }

        public async Task<ulong> UpdateCalendarDefaultChannelAsync(ulong calendarId, ulong newDefaultChannel)
        {
            var calendar = await _context.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
            if (calendar == null)
            {
                throw new CalendarNotFoundException();
            }

            calendar.DefaultChannel = newDefaultChannel;
            await _context.SaveChangesAsync();
            
            return calendar.DefaultChannel;
        }

        public async Task<string> GetCalendarTimezoneAsync(ulong calendarId)
        {
            return await _context.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Timezone)
                .FirstOrDefaultAsync();
        }

        public async Task<string> UpdateCalendarTimezoneAsync(ulong calendarId, string newTimezone)
        {
            var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(newTimezone);
            if (tz == null)
            {
                throw new InvalidTimeZoneException("Invalid TZ timezone");
            }

            var calendar = await _context.Calendars
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.Id == calendarId);
            if (calendar == null || string.IsNullOrEmpty(calendar.Timezone))
            {
                throw new CalendarNotFoundException();
            }

            var oldTz = DateTimeZoneProviders.Tzdb[calendar.Timezone];
            var earliestEvent = calendar.Events.OrderBy(e => e.StartTimestamp).FirstOrDefault();
            if (earliestEvent != null)
            {
                Instant instant = Instant.FromDateTimeOffset(earliestEvent.StartTimestamp);
                LocalDateTime dt = new ZonedDateTime(instant, oldTz).LocalDateTime;
                ZonedDateTime zdt = tz.AtStrictly(dt);
                if (zdt.ToInstant().ToDateTimeOffset() < SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset())
                {
                    throw new ExistingEventInNewTimezonePastException();
                }
            }

            calendar.Timezone = newTimezone;
            foreach (var evt in calendar.Events)
            {
                Instant startInstant = Instant.FromDateTimeOffset(evt.StartTimestamp);
                Instant endInstant = Instant.FromDateTimeOffset(evt.EndTimestamp);
                LocalDateTime startDt = new ZonedDateTime(startInstant, oldTz).LocalDateTime;
                LocalDateTime endDt = new ZonedDateTime(endInstant, oldTz).LocalDateTime;
                ZonedDateTime startZdt = tz.AtStrictly(startDt);
                ZonedDateTime endZdt = tz.AtStrictly(endDt);
                evt.StartTimestamp = startZdt.ToDateTimeOffset();
                evt.EndTimestamp = endZdt.ToDateTimeOffset();
            }
            await _context.SaveChangesAsync();

            return calendar.Timezone;
        }

        public async Task<bool?> InitialiseCalendar(ulong calendarId, string timezone, ulong defaultChannelId)
        {
            var calendar = await _context.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
            if (calendar == null)
            {
                calendar = new Calendar
                {
                    Id = calendarId,
                    Prefix = _defaultPrefix,
                    Events = new List<Event>()
                };
                await _context.Calendars.AddAsync(calendar);
                await _context.SaveChangesAsync();
            }

            if (!string.IsNullOrEmpty(calendar.Timezone))
            {
                return null;
            }
            var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timezone);
            if (tz == null)
            {
                return false;
            }
            calendar.Timezone = timezone;
            calendar.DefaultChannel = defaultChannelId;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Calendar> TryGetCalendarAsync(ulong calendarId)
        {
            return await _context.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
        }
    }
}
