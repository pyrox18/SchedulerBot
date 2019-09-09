using System;
using System.Threading;
using System.Threading.Tasks;
using SchedulerBot.Application.Events.Models;

namespace SchedulerBot.Client.Scheduler
{
    public interface IEventScheduler
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
        Task ScheduleEvent(EventViewModel evt, int clientShardId, ulong channelId);
        Task UnscheduleEvent(EventViewModel evt);
        Task UnscheduleEvent(Guid eventId);
        Task RescheduleEvent(EventViewModel evt, int clientShardId, ulong channelId);
    }
}
