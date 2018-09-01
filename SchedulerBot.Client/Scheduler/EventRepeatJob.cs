using System;
using System.Threading.Tasks;
using DSharpPlus;
using Quartz;
using RedLockNet;
using SchedulerBot.Client.Exceptions;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Scheduler
{
    public class EventRepeatJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap jobDataMap = context.MergedJobDataMap;
            var eventId = (Guid)jobDataMap["eventId"];
            var client = (DiscordClient)jobDataMap["client"];
            var channelId = (ulong)jobDataMap["channelId"];
            var eventService = (IEventService)jobDataMap["eventService"];
            var eventScheduler = (IEventScheduler)jobDataMap["eventScheduler"];
            var redlockFactory = (IDistributedLockFactory)jobDataMap["redlockFactory"];

            Event evt;
            var guildId = (await client.GetChannelAsync(channelId)).GuildId;
            using (var redlock = await redlockFactory.CreateLockAsync(guildId.ToString(), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(0.5)))
            {
                if (redlock.IsAcquired)
                {
                    evt = await eventService.ApplyRepeatAsync(eventId);
                }
                else
                {
                    throw new RedisLockAcquireException($"Cannot acquire lock for guild {guildId}");
                }
            }

            await eventScheduler.ScheduleEvent(evt, client, channelId);
        }
    }
}
