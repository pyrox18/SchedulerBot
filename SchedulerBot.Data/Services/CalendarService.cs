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
        private readonly SchedulerBotContextFactory _contextFactory;
        private readonly string _defaultPrefix;

        public CalendarService(SchedulerBotContextFactory contextFactory)
        {
            _contextFactory = contextFactory;

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
            using (var db = _contextFactory.CreateDbContext())
            {
                await db.Calendars.AddAsync(calendar);
                await db.SaveChangesAsync();
            }

            return calendar;
        }

        public async Task<bool> DeleteCalendarAsync(ulong calendarId)
        {
            using (var db = _contextFactory.CreateDbContext())
            {
                var permissionsToRemove = await db.Permissions.Where(p => p.Calendar.Id == calendarId).ToListAsync();
                var calendarToRemove = await db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);

                db.Permissions.RemoveRange(permissionsToRemove);
                db.Calendars.Remove(calendarToRemove);
                await db.SaveChangesAsync();
            }

            return true;
        }

        public async Task<string> GetCalendarPrefixAsync(ulong calendarId)
        {
            string prefix;

            using (var db = _contextFactory.CreateDbContext())
            {
                prefix = await db.Calendars
                    .Where(c => c.Id == calendarId)
                    .Select(c => c.Prefix)
                    .FirstOrDefaultAsync();
            }

            return prefix;
        }

        public async Task<string> UpdateCalendarPrefixAsync(ulong calendarId, string newPrefix)
        {
            Calendar calendar;

            using (var db = _contextFactory.CreateDbContext())
            {
                calendar = await db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
                if (calendar == null)
                {
                    throw new CalendarNotFoundException();
                }

                calendar.Prefix = newPrefix;
                await db.SaveChangesAsync();
            }

            return calendar.Prefix;
        }

        public async Task<ulong> GetCalendarDefaultChannelAsync(ulong calendarId)
        {
            ulong defaultChannel;

            using (var db = _contextFactory.CreateDbContext())
            {
                defaultChannel = await db.Calendars
                    .Where(c => c.Id == calendarId)
                    .Select(c => c.DefaultChannel)
                    .FirstOrDefaultAsync();
            }

            return defaultChannel;
        }

        public async Task<ulong> UpdateCalendarDefaultChannelAsync(ulong calendarId, ulong newDefaultChannel)
        {
            Calendar calendar;

            using (var db = _contextFactory.CreateDbContext())
            {
                calendar = await db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
                if (calendar == null)
                {
                    throw new CalendarNotFoundException();
                }

                calendar.DefaultChannel = newDefaultChannel;
                await db.SaveChangesAsync();
            }
            
            return calendar.DefaultChannel;
        }

        public async Task<string> GetCalendarTimezoneAsync(ulong calendarId)
        {
            string timezone;

            using (var db = _contextFactory.CreateDbContext())
            {
                timezone = await db.Calendars
                    .Where(c => c.Id == calendarId)
                    .Select(c => c.Timezone)
                    .FirstOrDefaultAsync();
            }

            return timezone;
        }

        public async Task<string> UpdateCalendarTimezoneAsync(ulong calendarId, string newTimezone)
        {
            var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(newTimezone);
            if (tz == null)
            {
                throw new InvalidTimeZoneException("Invalid TZ timezone");
            }

            Calendar calendar;
            
            using (var db = _contextFactory.CreateDbContext())
            {
                calendar = await db.Calendars
                    .Include(c => c.Events)
                    .FirstOrDefaultAsync(c => c.Id == calendarId);
                if (calendar == null)
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
                await db.SaveChangesAsync();
            }

            return calendar.Timezone;
        }

        public async Task<bool?> InitialiseCalendar(ulong calendarId, string timezone, ulong defaultChannelId)
        {
            using (var db = _contextFactory.CreateDbContext())
            {
                var calendar = await db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
                if (calendar == null)
                {
                    calendar = new Calendar
                    {
                        Id = calendarId,
                        Prefix = _defaultPrefix,
                        Events = new List<Event>()
                    };
                    await db.Calendars.AddAsync(calendar);
                    await db.SaveChangesAsync();
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
                await db.SaveChangesAsync();
            }
            return true;
        }

        public async Task<Calendar> TryGetCalendarAsync(ulong calendarId)
        {
            using (var db = _contextFactory.CreateDbContext())
            {
                return await db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
            }
        }
    }
}
