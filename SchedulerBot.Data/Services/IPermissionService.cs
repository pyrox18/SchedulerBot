using System.Collections.Generic;
using System.Threading.Tasks;
using SchedulerBot.Data.Enumerations;
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
        Task<List<Permission>> GetPermissionsForNodeAsync(ulong calendarId, string node);
        Task<List<Permission>> GetPermissionsForRoleAsync(ulong calendarId, ulong roleId);
        Task<List<Permission>> GetPermissionsForUserAsync(ulong calendarId, ulong userId);
        Task<bool> CheckPermissionsAsync(PermissionNode node, ulong calendarId, ulong userId, IEnumerable<ulong> roleIds);
    }
}
