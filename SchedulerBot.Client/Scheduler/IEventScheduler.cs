using System;
using System.Threading.Tasks;
using DSharpPlus;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Client.Scheduler
{
    public interface IEventScheduler
    {
        Task Start();
        Task Shutdown();
        Task PollAndScheduleEvents(DiscordClient client);
        Task ScheduleEvent(Event evt, DiscordClient client, ulong channelId, ulong? guildId = null);
        Task ScheduleEvent(EventViewModel evt, DiscordClient client, ulong channelId, ulong? guildId = null);
        Task UnscheduleEvent(Event evt);
        Task UnscheduleEvent(Guid eventId);
        Task RescheduleEvent(Event evt, DiscordClient client, ulong channelId);
        Task RescheduleEvent(EventViewModel evt, DiscordClient client, ulong channelId);
    }
}
