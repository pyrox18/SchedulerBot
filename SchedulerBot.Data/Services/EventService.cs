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
    }
}
