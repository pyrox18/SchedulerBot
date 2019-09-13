using System.Threading.Tasks;
using SchedulerBot.Domain.Models;

namespace SchedulerBot.Application.Interfaces
{
    public interface ICalendarRepository : IAsyncRepository<Calendar>
    {
        Task<Calendar> GetByIdAsync(ulong id);
        Task DeleteAllEventsAsync(ulong id);
    }
}