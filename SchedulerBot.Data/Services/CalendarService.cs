using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Data.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly SchedulerBotContext _db;

        public CalendarService(SchedulerBotContext context) => _db = context;

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
    }
}
