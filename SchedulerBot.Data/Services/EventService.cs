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
        private readonly SchedulerBotContextFactory _contextFactory;

        public EventService(SchedulerBotContextFactory contextFactory) => _contextFactory = contextFactory;

        public async Task<Event> CreateEventAsync(ulong calendarId, Event evt)
        {
            using (var db = _contextFactory.CreateDbContext())
            {
                var calendar = await db.Calendars
                    .Include(c => c.Events)
                    .FirstOrDefaultAsync(c => c.Id == calendarId);
                if (calendar == null)
                {
                    throw new CalendarNotFoundException();
                }

                evt.Id = Guid.NewGuid();
                calendar.Events.Add(evt);
                await db.SaveChangesAsync();
            }

            return evt;
        }

        public async Task<List<Event>> GetEventsAsync(ulong calendarId)
        {
            List<Event> orderedEvents;
            string timezone;

            using (var db = _contextFactory.CreateDbContext())
            {
                var isCalendarExists = await db.Calendars.AnyAsync(c => c.Id == calendarId);
                if (!isCalendarExists)
                {
                    throw new CalendarNotFoundException();
                }

                var events = await db.Events
                    .Include(e => e.Mentions)
                    .Where(e => e.Calendar.Id == calendarId)
                    .ToListAsync();

                timezone = await db.Calendars
                    .Where(c => c.Id == calendarId)
                    .Select(c => c.Timezone)
                    .FirstOrDefaultAsync();

                orderedEvents = events.OrderBy(e => e.StartTimestamp).ToList();
            }

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

            using (var db = _contextFactory.CreateDbContext())
            {
                var isCalendarExists = await db.Calendars.AnyAsync(c => c.Id == calendarId);
                if (!isCalendarExists)
                {
                    throw new CalendarNotFoundException();
                }

                var events = await db.Calendars
                    .Where(c => c.Id == calendarId)
                    .Select(c => c.Events)
                    .FirstOrDefaultAsync();
                events = events.OrderBy(e => e.StartTimestamp).ToList();

                deletedEvent = await db.Events
                    .Include(e => e.Mentions)
                    .Include(e => e.RSVPs)
                    .FirstOrDefaultAsync(e => e.Id == events[index].Id);
                db.EventMentions.RemoveRange(deletedEvent.Mentions);
                db.EventRSVPs.RemoveRange(deletedEvent.RSVPs);
                db.Events.Remove(deletedEvent);
                await db.SaveChangesAsync();

                timezone = await db.Calendars
                    .Where(c => c.Id == calendarId)
                    .Select(c => c.Timezone)
                    .FirstOrDefaultAsync();
            }

            AdjustTimestampsToTimezone(ref deletedEvent, timezone);

            return deletedEvent;
        }

        public async Task<Event> DeleteEventAsync(Guid eventId)
        {
            Event evt;

            using (var db = _contextFactory.CreateDbContext())
            {
                evt = await db.Events
                    .Include(e => e.Mentions)
                    .Include(e => e.RSVPs)
                    .FirstOrDefaultAsync(e => e.Id == eventId);
                db.EventMentions.RemoveRange(evt.Mentions);
                db.EventRSVPs.RemoveRange(evt.RSVPs);
                db.Events.Remove(evt);
                await db.SaveChangesAsync();
            }

            return evt;
        }

        public async Task<List<Event>> DeleteAllEventsAsync(ulong calendarId)
        {
            List<Event> events;

            using (var db = _contextFactory.CreateDbContext())
            {
                var isCalendarExists = await db.Calendars.AnyAsync(c => c.Id == calendarId);
                if (!isCalendarExists)
                {
                    throw new CalendarNotFoundException();
                }

                events = await db.Events
                    .Where(e => e.Calendar.Id == calendarId)
                    .ToListAsync();

                var mentions = await db.EventMentions
                    .Where(m => m.Event.Calendar.Id == calendarId)
                    .ToListAsync();

                var rsvps = await db.EventRSVPs
                    .Where(r => r.Event.Calendar.Id == calendarId)
                    .ToListAsync();

                db.Events.RemoveRange(events);
                db.EventMentions.RemoveRange(mentions);
                db.EventRSVPs.RemoveRange(rsvps);
                await db.SaveChangesAsync();
            }

            return events;
        }

        public async Task<Event> GetEventByIndexAsync(ulong calendarId, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("Index must be greater than 0");
            }

            Event evt;
            string timezone;

            using (var db = _contextFactory.CreateDbContext())
            {
                var isCalendarExists = await db.Calendars.AnyAsync(c => c.Id == calendarId);
                if (!isCalendarExists)
                {
                    throw new CalendarNotFoundException();
                }

                var events = await db.Calendars
                    .Where(c => c.Id == calendarId)
                    .Select(c => c.Events)
                    .FirstOrDefaultAsync();
                events = events.OrderBy(e => e.StartTimestamp).ToList();

                evt = events[index];
                evt = await db.Events
                    .Include(e => e.Mentions)
                    .Include(e => e.RSVPs)
                    .Where(e => e == evt)
                    .FirstOrDefaultAsync();

                timezone = await db.Calendars
                    .Where(c => c.Id == calendarId)
                    .Select(c => c.Timezone)
                    .FirstOrDefaultAsync();
            }

            AdjustTimestampsToTimezone(ref evt, timezone);

            return evt;
        }

        public async Task<Event> UpdateEventAsync(Event evt)
        {
            Event eventInDb;
            string timezone;

            using (var db = _contextFactory.CreateDbContext())
            {
                var mentionsToDelete = await db.EventMentions
                    .Where(m => m.Event.Id == evt.Id)
                    .ToListAsync();
                db.EventMentions.RemoveRange(mentionsToDelete);
                await db.SaveChangesAsync();

                eventInDb = await db.Events
                    .Include(e => e.Calendar)
                    .Include(e => e.Mentions)
                    .FirstOrDefaultAsync(e => e.Id == evt.Id);
                eventInDb.Name = evt.Name;
                eventInDb.StartTimestamp = evt.StartTimestamp;
                eventInDb.EndTimestamp = evt.EndTimestamp;
                eventInDb.Description = evt.Description;
                eventInDb.Repeat = evt.Repeat;
                eventInDb.Mentions = evt.Mentions;

                await db.SaveChangesAsync();

                timezone = await db.Calendars
                    .Where(c => c.Id == eventInDb.Calendar.Id)
                    .Select(c => c.Timezone)
                    .FirstOrDefaultAsync();
            }

            AdjustTimestampsToTimezone(ref eventInDb, timezone);

            return eventInDb;
        }

        public async Task<Event> ApplyRepeatAsync(Guid eventId)
        {
            Event evt;

            using (var db = _contextFactory.CreateDbContext())
            {
                evt = await db.Events
                    .Include(e => e.Calendar)
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
                    case RepeatType.None:
                    default:
                        break;
                }

                await db.SaveChangesAsync();
            }

            AdjustTimestampsToTimezone(ref evt, evt.Calendar.Timezone);

            return evt;
        }

        public async Task<List<Event>> GetEventsInHourIntervalAsync(double hours)
        {
            List<Event> events;

            using (var db = _contextFactory.CreateDbContext())
            {
                events = await db.Events
                    .Include(e => e.Calendar)
                    .Include(e => e.Mentions)
                    .Where(e => e.StartsInHours(hours) || e.RemindInHours(hours))
                    .ToListAsync();
            }

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
            List<Event> events;

            using (var db = _contextFactory.CreateDbContext())
            {
                events = await db.Events
                    .Include(e => e.Calendar)
                    .Include(e => e.Mentions)
                    .Where(e => guildIds.Contains(e.Calendar.Id) && (e.StartsInHours(hours) || e.RemindInHours(hours)))
                    .ToListAsync();
            }

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
            Event evt;
            string timezone;

            using (var db = _contextFactory.CreateDbContext())
            {
                var isCalendarExists = await db.Calendars.AnyAsync(c => c.Id == calendarId);
                if (!isCalendarExists)
                {
                    throw new CalendarNotFoundException();
                }

                var events = await db.Calendars
                    .Where(c => c.Id == calendarId)
                    .Select(c => c.Events)
                    .FirstOrDefaultAsync();
                events = events.OrderBy(e => e.StartTimestamp).ToList();

                evt = events[index];
                evt = await db.Events
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
                    db.EventRSVPs.Remove(rsvp);
                }

                await db.SaveChangesAsync();

                timezone = await db.Calendars
                    .Where(c => c.Id == calendarId)
                    .Select(c => c.Timezone)
                    .FirstOrDefaultAsync();
            }

            AdjustTimestampsToTimezone(ref evt, timezone);

            return evt;
        }

        public async Task ApplyDeleteAndRepeatPastEventsAsync()
        {
            using (var db = _contextFactory.CreateDbContext())
            {
                var events = await db.Events
                    .Include(e => e.Mentions)
                    .Include(e => e.RSVPs)
                    .Where(e => e.EndTimestamp < DateTimeOffset.Now)
                    .ToListAsync();

                foreach (var evt in events)
                {
                    if (evt.Repeat == RepeatType.None)
                    {
                        db.EventMentions.RemoveRange(evt.Mentions);
                        db.EventRSVPs.RemoveRange(evt.RSVPs);
                        db.Events.Remove(evt);
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
                                default:
                                    break;
                            }
                        }
                    }
                }

                await db.SaveChangesAsync();
            }

        }

        private void AdjustTimestampsToTimezone(ref Event evt, string timezone)
        {
            var tz = DateTimeZoneProviders.Tzdb[timezone];
            Instant instant = Instant.FromDateTimeOffset(evt.StartTimestamp);
            LocalDateTime dt = new ZonedDateTime(instant, DateTimeZoneProviders.Tzdb[timezone]).LocalDateTime;
            ZonedDateTime zdt = tz.AtStrictly(dt);
            evt.StartTimestamp = zdt.ToDateTimeOffset();

            instant = Instant.FromDateTimeOffset(evt.EndTimestamp);
            dt = new ZonedDateTime(instant, DateTimeZoneProviders.Tzdb[timezone]).LocalDateTime;
            zdt = tz.AtStrictly(dt);
            evt.EndTimestamp = zdt.ToDateTimeOffset();
        }
    }
}
