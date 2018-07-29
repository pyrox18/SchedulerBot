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
                .Select(c => c.Events)
                .FirstOrDefaultAsync();
        }

        public async Task<Event> DeleteEventAsync(ulong calendarId, int index)
        {
            if (index <= 0)
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
            events.RemoveAt(index);
            await _db.SaveChangesAsync();
            return deletedEvent;
        }
    }
}
