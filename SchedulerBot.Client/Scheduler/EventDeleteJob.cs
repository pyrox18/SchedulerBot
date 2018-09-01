using System;
using System.Threading.Tasks;
using DSharpPlus;
using Quartz;
using RedLockNet;
using SchedulerBot.Client.Exceptions;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Scheduler
{
    public class EventDeleteJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap jobDataMap = context.MergedJobDataMap;
            DiscordClient client = (DiscordClient)jobDataMap["client"];
            Guid eventId = (Guid)jobDataMap["eventId"];
            ulong channelId = (ulong)jobDataMap["channelId"];
            IEventService eventService = (IEventService)jobDataMap["eventService"];
            IDistributedLockFactory redlockFactory = (IDistributedLockFactory)jobDataMap["redlockFactory"];

            var guildId = (await client.GetChannelAsync(channelId)).GuildId;

            using (var redlock = await redlockFactory.CreateLockAsync(guildId.ToString(), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(0.5)))
            {
                if (redlock.IsAcquired)
                {
                    await eventService.DeleteEventAsync(eventId);
                }
                else
                {
                    throw new RedisLockAcquireException($"Cannot acquire lock for guild {guildId}");
                }
            }
        }
    }
}
