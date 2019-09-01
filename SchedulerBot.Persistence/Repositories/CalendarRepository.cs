using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;
using SchedulerBot.Persistence.Specifications;

namespace SchedulerBot.Persistence.Repositories
{
    public class CalendarRepository : ICalendarRepository
    {
        private readonly SchedulerBotDbContext _context;

        public CalendarRepository(SchedulerBotDbContext context)
        {
            _context = context;
        }

        public async Task<Calendar> AddAsync(Calendar entity)
        {
            await _context.Calendars.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<int> CountAsync(ISpecification<Calendar> spec)
        {
            return await ApplySpecification(spec).CountAsync();
        }

        public async Task DeleteAllEventsAsync(ulong id)
        {
            var events= await _context.Events
                .Where(e => e.Calendar.Id == id)
                .ToListAsync();

            var mentions = await _context.EventMentions
                .Where(e => e.Event.Calendar.Id == id)
                .ToListAsync();

            var rsvps = await _context.EventRSVPs
                .Where(e => e.Event.Calendar.Id == id)
                .ToListAsync();

            _context.EventMentions.RemoveRange(mentions);
            _context.EventRSVPs.RemoveRange(rsvps);
            _context.Events.RemoveRange(events);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Calendar entity)
        {
            _context.Calendars.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Calendar> GetByIdAsync(ulong id)
        {
            return await _context.Calendars.FindAsync(id);
        }

        public Task<Calendar> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<Calendar>> ListAllAsync()
        {
            return await _context.Calendars.ToListAsync();
        }

        public async Task<IReadOnlyList<Calendar>> ListAsync(ISpecification<Calendar> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task UpdateAsync(Calendar entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        private IQueryable<Calendar> ApplySpecification(ISpecification<Calendar> specification)
        {
            return CalendarSpecificationEvaluator.GetQuery(_context.Calendars.AsQueryable(), specification);
        }
    }
}
