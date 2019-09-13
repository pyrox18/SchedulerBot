using SchedulerBot.Domain.Models;

namespace SchedulerBot.Application.Interfaces
{
    public interface IEventRepository : IAsyncRepository<Event>
    {
    }
}