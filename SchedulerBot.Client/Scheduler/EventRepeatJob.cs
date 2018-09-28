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

            Event evt;
            evt = await eventService.ApplyRepeatAsync(eventId);

            await eventScheduler.ScheduleEvent(evt, client, channelId);
        }
    }
}
