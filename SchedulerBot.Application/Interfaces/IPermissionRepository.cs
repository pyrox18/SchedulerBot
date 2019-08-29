using SchedulerBot.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchedulerBot.Application.Interfaces
{
    public interface IPermissionRepository : IAsyncRepository<Permission>
    {
        Task<List<Permission>> GetForUserAsync(ulong calendarId, ulong userId);
    }
}
