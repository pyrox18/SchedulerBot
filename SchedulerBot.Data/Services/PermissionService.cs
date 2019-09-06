using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SchedulerBot.Data.Enumerations;
using SchedulerBot.Data.Exceptions;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Data.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly SchedulerBotContextFactory _contextFactory;

        public PermissionService(SchedulerBotContextFactory contextFactory) => _contextFactory = contextFactory;

        public List<string> GetPermissionNodes()
        {
            return new List<string>(Enum.GetNames(typeof(PermissionNode)));
        }

        public async Task<bool> RemoveUserPermissionsAsync(ulong calendarId, ulong userId)
        {
            using (var db = _contextFactory.CreateDbContext())
            {
                var permissions = await db.Permissions
                    .Where(p => p.Calendar.Id == calendarId && p.Type == PermissionType.User && p.TargetId == userId)
                    .ToListAsync();

                db.Permissions.RemoveRange(permissions);
                await db.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> RemoveRolePermissionsAsync(ulong calendarId, ulong roleId)
        {
            using (var db = _contextFactory.CreateDbContext())
            {
                var permissions = await db.Permissions
                    .Where(p => p.Calendar.Id == calendarId && (p.Type == PermissionType.Role || p.Type == PermissionType.Everyone) && p.TargetId == roleId)
                    .ToListAsync();

                db.Permissions.RemoveRange(permissions);
                await db.SaveChangesAsync();

            }

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

            Permission permission;

            using (var db = _contextFactory.CreateDbContext())
            {
                var existingPermission = await db.Permissions
                    .FirstOrDefaultAsync(
                        p => p.Calendar.Id == calendarId
                        && (p.Type == PermissionType.Role || p.Type == PermissionType.Everyone)
                        && p.Node == (PermissionNode)Enum.Parse(typeof(PermissionNode), actualNode)
                        && p.TargetId == roleId
                    );

                if (existingPermission != null)
                {
                    return existingPermission;
                }

                permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    Calendar = await db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId),
                    Type = roleId == calendarId ? PermissionType.Everyone : PermissionType.Role,
                    Node = (PermissionNode)Enum.Parse(typeof(PermissionNode), actualNode),
                    TargetId = roleId,
                    IsDenied = true
                };

                await db.Permissions.AddAsync(permission);
                await db.SaveChangesAsync();
            }

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

            Permission permission;

            using (var db = _contextFactory.CreateDbContext())
            {
                permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    Calendar = await db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId),
                    Type = PermissionType.User,
                    Node = (PermissionNode)Enum.Parse(typeof(PermissionNode), actualNode),
                    TargetId = userId,
                    IsDenied = true
                };

                await db.Permissions.AddAsync(permission);
                await db.SaveChangesAsync();
            }

            return permission;
        }

        public async Task<Permission> AllowNodeForRoleAsync(ulong calendarId, ulong roleId, string node)
        {
            var nodes = Enum.GetNames(typeof(PermissionNode));
            var actualNode = nodes.FirstOrDefault(n => n.ToLower() == node.ToLower());
            if (string.IsNullOrEmpty(actualNode))
            {
                throw new PermissionNodeNotFoundException();
            }

            Permission existingPermission;

            using (var db = _contextFactory.CreateDbContext())
            {
                existingPermission = await db.Permissions
                    .FirstOrDefaultAsync(
                        p => p.Calendar.Id == calendarId
                        && (p.Type == PermissionType.Role || p.Type == PermissionType.Everyone)
                        && p.Node == (PermissionNode)Enum.Parse(typeof(PermissionNode), actualNode) 
                        && p.TargetId == roleId
                    );

                if (existingPermission == null)
                {
                    existingPermission = new Permission
                    {
                        Node = (PermissionNode)Enum.Parse(typeof(PermissionNode), actualNode),
                        IsDenied = false
                    };
                    return existingPermission;
                }

                db.Permissions.Remove(existingPermission);
                await db.SaveChangesAsync();
                existingPermission.IsDenied = false;
            }

            return existingPermission;
        }

        public async Task<Permission> AllowNodeForUserAsync(ulong calendarId, ulong userId, string node)
        {
            var nodes = Enum.GetNames(typeof(PermissionNode));
            var actualNode = nodes.FirstOrDefault(n => n.ToLower() == node.ToLower());
            if (string.IsNullOrEmpty(actualNode))
            {
                throw new PermissionNodeNotFoundException();
            }

            Permission existingPermission;

            using (var db = _contextFactory.CreateDbContext())
            {
                existingPermission = await db.Permissions
                    .FirstOrDefaultAsync(
                        p => p.Calendar.Id == calendarId 
                        && p.Type == PermissionType.User
                        && p.Node == (PermissionNode)Enum.Parse(typeof(PermissionNode), actualNode) 
                        && p.TargetId == userId
                    );

                if (existingPermission == null)
                {
                    existingPermission.Node = (PermissionNode)Enum.Parse(typeof(PermissionNode), actualNode);
                    existingPermission.IsDenied = false;
                    return existingPermission;
                }

                db.Permissions.Remove(existingPermission);
                await db.SaveChangesAsync();
                existingPermission.IsDenied = false;
            }

            return existingPermission;
        }

        public async Task<List<Permission>> GetPermissionsForNodeAsync(ulong calendarId, string node)
        {
            var nodes = Enum.GetNames(typeof(PermissionNode));
            var actualNode = nodes.FirstOrDefault(n => n.ToLower() == node.ToLower());
            if (string.IsNullOrEmpty(actualNode))
            {
                throw new PermissionNodeNotFoundException();
            }

            List<Permission> permissions;

            using (var db = _contextFactory.CreateDbContext())
            {
                permissions = await db.Permissions
                    .Where(p => p.Calendar.Id == calendarId && p.Node == (PermissionNode)Enum.Parse(typeof(PermissionNode), actualNode))
                    .ToListAsync();
            }

            return permissions;
        }

        public async Task<List<Permission>> GetPermissionsForRoleAsync(ulong calendarId, ulong roleId)
        {
            List<Permission> permissions;
            
            using (var db = _contextFactory.CreateDbContext())
            {
                permissions = await db.Permissions
                    .Where(p => p.Calendar.Id == calendarId && (p.Type == PermissionType.Role || p.Type == PermissionType.Everyone) && p.TargetId == roleId)
                    .OrderBy(p => p.Node)
                    .ToListAsync();
            }

            return permissions;
        }

        public async Task<List<Permission>> GetPermissionsForUserAsync(ulong calendarId, ulong userId)
        {
            List<Permission> permissions;

            using (var db = _contextFactory.CreateDbContext())
            {
                permissions = await db.Permissions
                    .Where(p => p.Calendar.Id == calendarId && p.Type == PermissionType.User && p.TargetId == userId)
                    .OrderBy(p => p.Node)
                    .ToListAsync();
            }

            return permissions;
        }

        public async Task<bool> CheckPermissionsAsync(PermissionNode node, ulong calendarId, ulong userId, IEnumerable<ulong> roleIds)
        {
            bool isNotPermitted;
            using (var db = _contextFactory.CreateDbContext())
            {
                isNotPermitted = await db.Permissions.AnyAsync(
                    p => p.Calendar.Id == calendarId
                    && (p.Node == node || p.Node == PermissionNode.All)
                    && ((p.Type == PermissionType.Everyone) || (p.Type == PermissionType.User && p.TargetId == userId) || (p.Type == PermissionType.Role && roleIds.Contains(p.TargetId)))
                );
            }

            return !isNotPermitted;
        }
    }
}
