using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SchedulerBot.Data.Services
{
    public interface IPermissionService
    {
        Task<bool> RemoveUserPermissionsAsync(ulong calendarId, ulong userId);
        Task<bool> RemoveRolePermissionsAsync(ulong calendarId, ulong roleId);
    }
}
