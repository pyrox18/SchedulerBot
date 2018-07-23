using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Data.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly SchedulerBotContext _db;

        public PermissionService(SchedulerBotContext context) => _db = context;

        public async Task<bool> RemoveUserPermissionsAsync(ulong calendarId, ulong userId)
        {
            var permissions = await _db.Permissions
                .Where(p => p.Calendar.Id == calendarId && p.Type == PermissionType.User && p.TargetId == userId)
                .ToListAsync();

            _db.Permissions.RemoveRange(permissions);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRolePermissionsAsync(ulong calendarId, ulong roleId)
        {
            var permissions = await _db.Permissions
                .Where(p => p.Calendar.Id == calendarId && p.Type == PermissionType.Role && p.TargetId == roleId)
                .ToListAsync();

            _db.Permissions.RemoveRange(permissions);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
