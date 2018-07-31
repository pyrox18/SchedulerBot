using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            
            return await _db.Calendars
                .Where(c => c.Id == calendarId)
                .Select(c => c.Events)
                .FirstOrDefaultAsync();
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

            var deletedEvent = events[index];
            _db.Events.Remove(deletedEvent);
            await _db.SaveChangesAsync();
            return deletedEvent;
        }

        public async Task<Event> DeleteEventAsync(Guid eventId)
        {
            var evt = await _db.Events.FirstOrDefaultAsync(e => e.Id == eventId);
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

            var evt = events[index];
            return evt;
        }

        public async Task<Event> UpdateEventAsync(Event evt)
        {
            var eventInDb = await _db.Events.FirstOrDefaultAsync(e => e.Id == evt.Id);
            eventInDb.Name = evt.Name;
            eventInDb.StartTimestamp = evt.StartTimestamp;
            eventInDb.EndTimestamp = evt.EndTimestamp;
            eventInDb.Description = evt.Description;
            eventInDb.Repeat = evt.Repeat;

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
    }
}
