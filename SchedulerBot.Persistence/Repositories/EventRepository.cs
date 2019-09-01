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
    public class EventRepository : IEventRepository
    {
        private readonly SchedulerBotDbContext _context;

        public EventRepository(SchedulerBotDbContext context)
        {
            _context = context;
        }

        public async Task<Event> AddAsync(Event entity)
        {
            await _context.Events.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<int> CountAsync(ISpecification<Event> spec)
        {
            return await ApplySpecification(spec).CountAsync();
        }

        public async Task DeleteAsync(Event entity)
        {
            _context.Events.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Event> GetByIdAsync(Guid id)
        {
            return await _context.Events.FindAsync(id);
        }

        public async Task<IReadOnlyList<Event>> ListAllAsync()
        {
            return await _context.Events.ToListAsync();
        }

        public async Task<IReadOnlyList<Event>> ListAsync(ISpecification<Event> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task UpdateAsync(Event entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        private IQueryable<Event> ApplySpecification(ISpecification<Event> specification)
        {
            return EventSpecificationEvaluator.GetQuery(_context.Events.AsQueryable(), specification);
        }
    }
}
