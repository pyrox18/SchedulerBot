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
        private readonly SchedulerBotContext _db;

        public EventService(SchedulerBotContext context) => _db = context;

        public async Task<Event> CreateEventAsync(ulong calendarId, Event evt)
        {
            var calendar = await _db.Calendars
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.Id == calendarId);
            if (calendar == null)
            {
                throw new CalendarNotFoundException();
            }

            evt.Id = Guid.NewGuid();
            calendar.Events.Add(evt);
            await _db.SaveChangesAsync();
            return evt;
        }

        public async Task<List<Event>> GetEventsAsync(ulong calendarId)
        {
            var isCalendarExists = await _db.Calendars.AnyAsync(c => c.Id == calendarId);
            if (!isCalendarExists)
            {
                throw new CalendarNotFoundException();
            }

            var events = await _db.Events
                .Include(e => e.Mentions)
                .Where(e => e.Calendar.Id == calendarId)
                .ToListAsync();

            var timezone = await _db.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Timezone)
                .FirstOrDefaultAsync();

            var orderedEvents = events.OrderBy(e => e.StartTimestamp).ToList();
            var tz = DateTimeZoneProviders.Tzdb[timezone];
            foreach (var evt in orderedEvents)
            {
                Instant instant = Instant.FromDateTimeOffset(evt.StartTimestamp);
                LocalDateTime dt = new ZonedDateTime(instant, DateTimeZoneProviders.Tzdb[timezone]).LocalDateTime;
                ZonedDateTime zdt = tz.AtStrictly(dt);
                evt.StartTimestamp = zdt.ToDateTimeOffset();

                instant = Instant.FromDateTimeOffset(evt.EndTimestamp);
                dt = new ZonedDateTime(instant, DateTimeZoneProviders.Tzdb[timezone]).LocalDateTime;
                zdt = tz.AtStrictly(dt);
                evt.EndTimestamp = zdt.ToDateTimeOffset();
            }

            return orderedEvents;
        }

        public async Task<Event> DeleteEventAsync(ulong calendarId, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("Index must be greater than 0");
            }

            var isCalendarExists = await _db.Calendars.AnyAsync(c => c.Id == calendarId);
            if (!isCalendarExists)
            {
                throw new CalendarNotFoundException();
            }

            var events = await _db.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Events)
                .FirstOrDefaultAsync();
            events = events.OrderBy(e => e.StartTimestamp).ToList();

            var deletedEvent = events[index];
            _db.EventMentions.RemoveRange(deletedEvent.Mentions);
            _db.Events.Remove(deletedEvent);
            await _db.SaveChangesAsync();
            return deletedEvent;
        }

        public async Task<Event> DeleteEventAsync(Guid eventId)
        {
            var evt = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
            _db.EventMentions.RemoveRange(evt.Mentions);
            _db.Events.Remove(evt);
            await _db.SaveChangesAsync();
            return evt;
        }

        public async Task<Event> GetEventByIndexAsync(ulong calendarId, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("Index must be greater than 0");
            }

            var isCalendarExists = await _db.Calendars.AnyAsync(c => c.Id == calendarId);
            if (!isCalendarExists)
            {
                throw new CalendarNotFoundException();
            }

            var events = await _db.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Events)
                .FirstOrDefaultAsync();
            events = events.OrderBy(e => e.StartTimestamp).ToList();

            var evt = events[index];
            evt = await _db.Events
                .Include(e => e.Mentions)
                .Include(e => e.RSVPs)
                .Where(e => e == evt)
                .FirstOrDefaultAsync();
            return evt;
        }

        public async Task<Event> UpdateEventAsync(Event evt)
        {
            var mentionsToDelete = await _db.EventMentions
                .Where(m => m.Event.Id == evt.Id)
                .ToListAsync();
            _db.EventMentions.RemoveRange(mentionsToDelete);
            await _db.SaveChangesAsync();

            var eventInDb = await _db.Events
                .Include(e => e.Mentions)
                .FirstOrDefaultAsync(e => e.Id == evt.Id);
            eventInDb.Name = evt.Name;
            eventInDb.StartTimestamp = evt.StartTimestamp;
            eventInDb.EndTimestamp = evt.EndTimestamp;
            eventInDb.Description = evt.Description;
            eventInDb.Repeat = evt.Repeat;
            eventInDb.Mentions = evt.Mentions;

            await _db.SaveChangesAsync();
            return eventInDb;
        }

        public async Task<Event> ApplyRepeatAsync(Guid eventId)
        {
            var evt = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
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

            await _db.SaveChangesAsync();
            return evt;
        }

        public async Task<List<Event>> GetEventsInHourIntervalAsync(double hours)
        {
            var events = await _db.Events
                .Include(e => e.Calendar)
                .Include(e => e.Mentions)
                .Where(e => e.StartTimestamp <= DateTimeOffset.Now.AddHours(hours))
                .ToListAsync();

            return events;
        }
    }
}
