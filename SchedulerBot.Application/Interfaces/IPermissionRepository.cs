using SchedulerBot.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchedulerBot.Application.Interfaces
{
    public interface IPermissionRepository : IAsyncRepository<Permission>
    {
        Task<List<Permission>> GetForUserAsync(ulong calendarId, ulong userId);
        Task<List<Permission>> GetForRoleAsync(ulong calendarId, ulong roleId);
        Task<List<Permission>> GetForNodeAsync(ulong calendarId, PermissionNode node);
    }
}
