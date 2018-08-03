using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;
using SchedulerBot.Data.Exceptions;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Data.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly SchedulerBotContext _db;
        private readonly string _defaultPrefix;

        public CalendarService(SchedulerBotContext context)
        {
            _db = context;

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var json = "";
            var filePath = $"appsettings.{environment}.json";
            if (environment == "Development")
            {
                filePath = string.Format("..{0}..{0}..{0}{1}", Path.DirectorySeparatorChar, filePath);
            }

            using (var fs = File.OpenRead(filePath))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            var config = JsonConvert.DeserializeAnonymousType(json, new
            {
                Bot = new
                {
                    Prefixes = new string[] { }
                }
            });

            _defaultPrefix = config.Bot.Prefixes[0];
        }

        public async Task<Calendar> CreateCalendarAsync(Calendar calendar)
        {
            await _db.Calendars.AddAsync(calendar);
            await _db.SaveChangesAsync();

            return calendar;
        }

        public async Task<bool> DeleteCalendarAsync(ulong calendarId)
        {
            var permissionsToRemove = await _db.Permissions.Where(p => p.Calendar.Id == calendarId).ToListAsync();
            var calendarToRemove = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);

            _db.Permissions.RemoveRange(permissionsToRemove);
            _db.Calendars.Remove(calendarToRemove);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<int> ResolveCalendarPrefixAsync(ulong calendarId, string message)
        {
            var prefix = await _db.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Prefix)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(prefix) && message.StartsWith(_defaultPrefix))
            {
                return _defaultPrefix.Length;
            }

            if (string.IsNullOrEmpty(prefix) || !message.StartsWith(prefix))
            {
                return -1;
            }

            return prefix.Length;
        }

        public async Task<string> GetCalendarPrefixAsync(ulong calendarId)
        {
            var prefix = await _db.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Prefix)
                .FirstOrDefaultAsync();

            return prefix;
        }

        public async Task<string> UpdateCalendarPrefixAsync(ulong calendarId, string newPrefix)
        {
            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
            if (calendar == null)
            {
                throw new CalendarNotFoundException();
            }

            calendar.Prefix = newPrefix;
            await _db.SaveChangesAsync();
            return calendar.Prefix;
        }

        public async Task<ulong> GetCalendarDefaultChannelAsync(ulong calendarId)
        {
            var defaultChannel = await _db.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.DefaultChannel)
                .FirstOrDefaultAsync();

            return defaultChannel;
        }

        public async Task<ulong> UpdateCalendarDefaultChannelAsync(ulong calendarId, ulong newDefaultChannel)
        {
            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
            if (calendar == null)
            {
                throw new CalendarNotFoundException();
            }

            calendar.DefaultChannel = newDefaultChannel;
            await _db.SaveChangesAsync();
            return calendar.DefaultChannel;
        }

        public async Task<string> GetCalendarTimezoneAsync(ulong calendarId)
        {
            var timezone = await _db.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Timezone)
                .FirstOrDefaultAsync();

            return timezone;
        }

        public async Task<string> UpdateCalendarTimezoneAsync(ulong calendarId, string newTimezone)
        {
            var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(newTimezone);
            if (tz == null)
            {
                throw new InvalidTimeZoneException("Invalid TZ timezone");
            }

            var calendar = await _db.Calendars
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.Id == calendarId);
            if (calendar == null)
            {
                throw new CalendarNotFoundException();
            }

            var earliestEvent = calendar.Events.OrderBy(e => e.StartTimestamp).FirstOrDefault();
            if (earliestEvent != null)
            {
                Instant instant = Instant.FromDateTimeOffset(earliestEvent.StartTimestamp);
                LocalDateTime dt = new ZonedDateTime(instant, DateTimeZoneProviders.Tzdb[calendar.Timezone]).LocalDateTime;
                ZonedDateTime zdt = tz.AtStrictly(dt);
                if (zdt.ToInstant().ToDateTimeOffset() < SystemClock.Instance.GetCurrentInstant().ToDateTimeOffset())
                {
                    throw new ExistingEventInNewTimezonePastException();
                }
            }

            calendar.Timezone = newTimezone;
            foreach (var evt in calendar.Events)
            {
                LocalDateTime startDt = LocalDateTime.FromDateTime(evt.StartTimestamp.LocalDateTime);
                LocalDateTime endDt = LocalDateTime.FromDateTime(evt.EndTimestamp.LocalDateTime);
                ZonedDateTime startZdt = tz.AtStrictly(startDt);
                ZonedDateTime endZdt = tz.AtStrictly(endDt);
                evt.StartTimestamp = startZdt.ToDateTimeOffset();
                evt.EndTimestamp = endZdt.ToDateTimeOffset();
            }
            await _db.SaveChangesAsync();
            return calendar.Timezone;
        }

        public async Task<bool?> InitialiseCalendar(ulong calendarId, string timezone, ulong defaultChannelId)
        {
            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
            if (calendar == null)
            {
                calendar = await CreateCalendarAsync(new Calendar
                {
                    Id = calendarId,
                    Prefix = _defaultPrefix,
                    Events = new List<Event>()
                });
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
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<Calendar> TryGetCalendarAsync(ulong calendarId)
        {
            return await _db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
        }
    }
}
