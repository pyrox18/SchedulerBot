using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulerBot.Data.Exceptions;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Data.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly SchedulerBotContext _db;

        public PermissionService(SchedulerBotContext context) => _db = context;

        public List<string> GetPermissionNodes()
        {
            return new List<string>(Enum.GetNames(typeof(PermissionNode)));
        }

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

        public async Task<Permission> DenyNodeForRoleAsync(ulong calendarId, ulong roleId, string node)
        {
            var nodes = Enum.GetNames(typeof(PermissionNode));
            var actualNode = nodes.FirstOrDefault(n => n.ToLower() == node.ToLower());
            if (string.IsNullOrEmpty(actualNode))
            {
                throw new PermissionNodeNotFoundException();
            }

            var existingPermission = await _db.Permissions
                .FirstOrDefaultAsync(
                    p => p.Calendar.Id == calendarId 
                    && p.Type == PermissionType.Role
                    && p.Node == Enum.Parse<PermissionNode>(actualNode) 
                    && p.TargetId == roleId
                );

            if (existingPermission != null)
            {
                return existingPermission;
            }

            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                Calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId),
                Type = PermissionType.Role,
                Node = Enum.Parse<PermissionNode>(actualNode),
                TargetId = roleId,
                IsDenied = true
            };

            await _db.Permissions.AddAsync(permission);
            await _db.SaveChangesAsync();
            return permission;
        }

        public async Task<Permission> DenyNodeForUserAsync(ulong calendarId, ulong userId, string node)
        {
            var nodes = Enum.GetNames(typeof(PermissionNode));
            var actualNode = nodes.FirstOrDefault(n => n.ToLower() == node.ToLower());
            if (string.IsNullOrEmpty(actualNode))
            {
                throw new PermissionNodeNotFoundException();
            }

            var permission = new Permission
            {
                Id = Guid.NewGuid(),
                Calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId),
                Type = PermissionType.User,
                Node = Enum.Parse<PermissionNode>(actualNode),
                TargetId = userId,
                IsDenied = true
            };

            await _db.Permissions.AddAsync(permission);
            await _db.SaveChangesAsync();
            return permission;
        }
    }
}
