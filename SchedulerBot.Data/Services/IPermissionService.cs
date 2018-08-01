using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Data.Services
{
    public interface IPermissionService
    {
        List<string> GetPermissionNodes();
        Task<bool> RemoveUserPermissionsAsync(ulong calendarId, ulong userId);
        Task<bool> RemoveRolePermissionsAsync(ulong calendarId, ulong roleId);
        Task<Permission> DenyNodeForRoleAsync(ulong calendarId, ulong roleId, string node);
        Task<Permission> DenyNodeForUserAsync(ulong calendarId, ulong userId, string node);
        Task<Permission> AllowNodeForRoleAsync(ulong calendarId, ulong roleId, string node);
        Task<Permission> AllowNodeForUserAsync(ulong calendarId, ulong userId, string node);
    }
}
