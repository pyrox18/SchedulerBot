using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;
using SchedulerBot.Persistence.Specifications;

namespace SchedulerBot.Persistence.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly SchedulerBotDbContext _context;

        public PermissionRepository(SchedulerBotDbContext context)
        {
            _context = context;
        }

        public async Task<Permission> AddAsync(Permission entity)
        {
            await _context.Permissions.AddAsync(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task AllowRolePermissionAsync(ulong calendarId, ulong roleId, PermissionNode node)
        {
            var existingPermission = await _context.Permissions
                .Where(p => p.Calendar.Id == calendarId)
                .Where(p => p.TargetId == roleId && (p.Type == PermissionType.Role || p.Type == PermissionType.Everyone))
                .Where(p => p.Node == node)
                .FirstOrDefaultAsync();

            if (existingPermission is null) return;

            _context.Permissions.Remove(existingPermission);
            await _context.SaveChangesAsync();
        }

        public async Task AllowUserPermissionAsync(ulong calendarId, ulong userId, PermissionNode node)
        {
            var existingPermission = await _context.Permissions
                .Where(p => p.Calendar.Id == calendarId)
                .Where(p => p.TargetId == userId && p.Type == PermissionType.User)
                .Where(p => p.Node == node)
                .FirstOrDefaultAsync();

            if (existingPermission is null) return;

            _context.Permissions.Remove(existingPermission);
            await _context.SaveChangesAsync();
        }

        public async Task<int> CountAsync(ISpecification<Permission> spec)
        {
            return await ApplySpecification(spec).CountAsync();
        }

        public async Task DeleteAsync(Permission entity)
        {
            _context.Permissions.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DenyRolePermissionAsync(ulong calendarId, ulong roleId, PermissionNode node)
        {
            var permissionExists = await _context.Permissions
                .Where(p => p.Calendar.Id == calendarId)
                .Where(p => p.TargetId == roleId && (p.Type == PermissionType.Role || p.Type == PermissionType.Everyone))
                .Where(p => p.Node == node)
                .AnyAsync();

            if (permissionExists) return;

            var calendar = await GetCalendar(calendarId);

            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                Calendar = calendar,
                Type = roleId == calendarId ? PermissionType.Everyone : PermissionType.Role,
                Node = node,
                TargetId = roleId,
                IsDenied = true
            };

            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();
        }

        public async Task DenyUserPermissionAsync(ulong calendarId, ulong userId, PermissionNode node)
        {
            var permissionExists = await _context.Permissions
                .Where(p => p.Calendar.Id == calendarId)
                .Where(p => p.TargetId == userId && p.Type == PermissionType.User)
                .Where(p => p.Node == node)
                .AnyAsync();

            if (permissionExists) return;

            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                Calendar = await _context.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId),
                Type = PermissionType.User,
                Node = node,
                TargetId = userId,
                IsDenied = true
            };

            await _context.Permissions.AddAsync(permission);
            await _context.SaveChangesAsync();
        }

        public async Task<Permission> GetByIdAsync(Guid id)
        {
            return await _context.Permissions.FindAsync(id);
        }

        public async Task<List<Permission>> GetForNodeAsync(ulong calendarId, PermissionNode node)
        {
            return await GetPermissionsForCalendar(calendarId)
                .Where(p => p.Node == node)
                .ToListAsync();
        }

        public async Task<List<Permission>> GetForRoleAsync(ulong calendarId, ulong roleId)
        {
            return await GetPermissionsForCalendar(calendarId)
                .Where(p => p.TargetId == roleId && (p.Type == PermissionType.Role || p.Type == PermissionType.Everyone))
                .ToListAsync();
        }

        public async Task<List<Permission>> GetForUserAsync(ulong calendarId, ulong userId)
        {
            return await GetPermissionsForCalendar(calendarId)
                .Where(p => p.TargetId == userId && p.Type == PermissionType.User)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Permission>> ListAllAsync()
        {
            return await _context.Permissions.ToListAsync();
        }

        public async Task<IReadOnlyList<Permission>> ListAsync(ISpecification<Permission> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task UpdateAsync(Permission entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        private IQueryable<Permission> GetPermissionsForCalendar(ulong calendarId)
        {
            return _context.Permissions.Where(p => p.Calendar.Id == calendarId);
        }

        private IQueryable<Permission> ApplySpecification(ISpecification<Permission> specification)
        {
            return PermissionSpecificationEvaluator.GetQuery(_context.Permissions.AsQueryable(), specification);
        }

        private async Task<Calendar> GetCalendar(ulong calendarId)
        {
            var calendar = await _context.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);

            if (calendar is null)
            {
                throw new CalendarNotInitialisedException(calendarId);
            }

            return calendar;
        }
    }
}
