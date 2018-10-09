using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Quartz;
using SchedulerBot.Client.Exceptions;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Scheduler
{
    public class EventDeleteJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap jobDataMap = context.MergedJobDataMap;
            Guid eventId = (Guid)jobDataMap["eventId"];
            IEventService eventService = (IEventService)jobDataMap["eventService"];
            var semaphore = (SemaphoreSlim)jobDataMap["semaphore"];

            await semaphore.WaitAsync();
            try
            {
                await eventService.DeleteEventAsync(eventId);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
