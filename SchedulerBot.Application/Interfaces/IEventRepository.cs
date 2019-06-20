using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Interfaces
{
    public interface IEventRepository : IAsyncRepository<Event>
    {
    }
}