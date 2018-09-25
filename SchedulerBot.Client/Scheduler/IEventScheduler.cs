using System;
using System.Threading.Tasks;
using DSharpPlus;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Client.Scheduler
{
    public interface IEventScheduler
    {
        Task Start();
        Task Shutdown();
        Task PollAndScheduleEvents(DiscordShardedClient client);
        Task ScheduleEvent(Event evt, DiscordClient client, ulong channelId, ulong? guildId = null);
        Task UnscheduleEvent(Event evt);
        Task UnscheduleEvent(Guid eventId);
        Task RescheduleEvent(Event evt, DiscordClient client, ulong channelId);
    }
}
