using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Quartz;
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
            var semaphore = (SemaphoreSlim)jobDataMap["semaphore"];

            Event evt;
            await semaphore.WaitAsync();
            try
            {
                evt = await eventService.ApplyRepeatAsync(eventId);
            }
            finally
            {
                semaphore.Release();
            }

            await eventScheduler.ScheduleEvent(evt, client, channelId);
        }
    }
}
