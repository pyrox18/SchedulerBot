using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Quartz;
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

            var evt = await eventService.ApplyRepeatAsync(eventId);
            await eventScheduler.ScheduleEvent(evt, client, channelId);
        }
    }
}
