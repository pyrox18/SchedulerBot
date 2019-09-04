using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Quartz;
using Quartz.Impl;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Scheduler
{
    public class EventScheduler : IEventScheduler
    {
        private readonly IEventService _eventService;
        private readonly SemaphoreSlim semaphore;
        public IScheduler Scheduler { get; set; }

        public EventScheduler(IEventService eventService)
        {
            _eventService = eventService;
            semaphore = new SemaphoreSlim(1, 1);
            InitialiseScheduler().GetAwaiter().GetResult();
        }

        private async Task InitialiseScheduler()
        {
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.scheduler.instanceName", "SchedulerBotScheduler" },
                { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
                { "quartz.threadPool.threadCount", "3" }
            };

            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            Scheduler = await factory.GetScheduler();
        }

        public async Task Start()
        {
            await Scheduler.Start();
        }

        public async Task Shutdown()
        {
            await Scheduler.Shutdown();
        }

        public async Task PollAndScheduleEvents(DiscordClient client)
        {
            var calendarIds = client.Guilds.Keys;
            var events = await _eventService.GetEventsInHourIntervalAsync(2, calendarIds);

            foreach (var evt in events)
            {
                var calendarId = calendarIds.FirstOrDefault(x => x == evt.Calendar.Id);
                await ScheduleEvent(evt, client, evt.Calendar.DefaultChannel, calendarId);
            }
        }

        public async Task ScheduleEvent(Event evt, DiscordClient client, ulong channelId, ulong? guildId = null)
        {
            if (await Scheduler.CheckExists(new TriggerKey(evt.Id.ToString(), "eventNotifications"))
                || await Scheduler.CheckExists(new TriggerKey(evt.Id.ToString(), "eventReminders"))
                || (!evt.StartsInHours(2) && (evt.ReminderTimestamp == null || !evt.RemindInHours(2))))
            {
                return;
            }

            DiscordChannel channel;
            try
            {
                channel = await client.GetChannelAsync(channelId);
            }
            catch (UnauthorizedException)
            {
                return;
            }
            catch (NotFoundException)
            {
                return;
            }

            var notifyJobDataMap = new JobDataMap
            {
                ["event"] = evt,
                ["client"] = client,
                ["channel"] = channel
            };

            if (!evt.HasStarted())
            {
                IJobDetail notifyJob = JobBuilder.Create<EventNotifyJob>()
                    .WithIdentity(evt.Id.ToString(), "eventNotifications")
                    .UsingJobData(notifyJobDataMap)
                    .Build();

                ITrigger notifyTrigger = TriggerBuilder.Create()
                    .WithIdentity(evt.Id.ToString(), "eventNotifications")
                    .StartAt(evt.StartTimestamp)
                    .ForJob(notifyJob)
                    .Build();

                if (!await Scheduler.CheckExists(notifyTrigger.Key))
                {
                    await Scheduler.ScheduleJob(notifyJob, notifyTrigger);
                }

                if (evt.ReminderTimestamp != null && !evt.HasReminderPassed())
                {
                    IJobDetail reminderJob = JobBuilder.Create<EventReminderJob>()
                        .WithIdentity(evt.Id.ToString(), "eventReminders")
                        .UsingJobData(notifyJobDataMap) // same data map as notify job
                        .Build();

                    ITrigger reminderTrigger = TriggerBuilder.Create()
                        .WithIdentity(evt.Id.ToString(), "eventReminders")
                        .StartAt(evt.ReminderTimestamp ?? DateTimeOffset.Now) // workaround for nullable timestamp
                        .ForJob(reminderJob)
                        .Build();

                    if (!await Scheduler.CheckExists(reminderTrigger.Key))
                    {
                        await Scheduler.ScheduleJob(reminderJob, reminderTrigger);
                    }
                }
            }

            if (evt.Repeat != RepeatType.None)
            {
                IJobDetail repeatJob = JobBuilder.Create<EventRepeatJob>()
                    .WithIdentity(evt.Id.ToString(), "eventRepeats")
                    .UsingJobData(new JobDataMap
                    {
                        ["eventId"] = evt.Id,
                        ["client"] = client,
                        ["channelId"] = channelId,
                        ["eventService"] = _eventService,
                        ["eventScheduler"] = this,
                        ["semaphore"] = semaphore
                    })
                    .Build();

                ITrigger repeatTrigger = TriggerBuilder.Create()
                    .WithIdentity(evt.Id.ToString(), "eventRepeats")
                    .StartAt(evt.EndTimestamp)
                    .ForJob(repeatJob)
                    .Build();

                if (!await Scheduler.CheckExists(repeatTrigger.Key))
                {
                    await Scheduler.ScheduleJob(repeatJob, repeatTrigger);
                }
            }
            else
            {
                var deleteJobDataMap = new JobDataMap
                {
                    ["client"] = client,
                    ["eventId"] = evt.Id,
                    ["eventService"] = _eventService,
                    ["semaphore"] = semaphore
                };

                IJobDetail deleteJob = JobBuilder.Create<EventDeleteJob>()
                    .WithIdentity(evt.Id.ToString(), "eventDeletions")
                    .UsingJobData(deleteJobDataMap)
                    .Build();

                ITrigger deleteTrigger = TriggerBuilder.Create()
                    .WithIdentity(evt.Id.ToString(), "eventDeletions")
                    .StartAt(evt.EndTimestamp)
                    .ForJob(deleteJob)
                    .Build();

                if (!await Scheduler.CheckExists(deleteTrigger.Key))
                {
                    await Scheduler.ScheduleJob(deleteJob, deleteTrigger);
                }
            }
        }

        public async Task ScheduleEvent(EventViewModel evt, DiscordClient client, ulong channelId, ulong? guildId = null)
        {
            if (await Scheduler.CheckExists(new TriggerKey(evt.Id.ToString(), "eventNotifications"))
                || await Scheduler.CheckExists(new TriggerKey(evt.Id.ToString(), "eventReminders"))
                || (!evt.StartsInHours(2) && (evt.ReminderTimestamp == null || !evt.RemindInHours(2))))
            {
                return;
            }

            DiscordChannel channel;
            try
            {
                channel = await client.GetChannelAsync(channelId);
            }
            catch (UnauthorizedException)
            {
                return;
            }
            catch (NotFoundException)
            {
                return;
            }

            var notifyJobDataMap = new JobDataMap
            {
                ["event"] = evt,
                ["client"] = client,
                ["channel"] = channel
            };

            if (!evt.HasStarted())
            {
                IJobDetail notifyJob = JobBuilder.Create<EventNotifyJob>()
                    .WithIdentity(evt.Id.ToString(), "eventNotifications")
                    .UsingJobData(notifyJobDataMap)
                    .Build();

                ITrigger notifyTrigger = TriggerBuilder.Create()
                    .WithIdentity(evt.Id.ToString(), "eventNotifications")
                    .StartAt(evt.StartTimestamp)
                    .ForJob(notifyJob)
                    .Build();

                if (!await Scheduler.CheckExists(notifyTrigger.Key))
                {
                    await Scheduler.ScheduleJob(notifyJob, notifyTrigger);
                }

                if (evt.ReminderTimestamp != null && !evt.HasReminderPassed())
                {
                    IJobDetail reminderJob = JobBuilder.Create<EventReminderJob>()
                        .WithIdentity(evt.Id.ToString(), "eventReminders")
                        .UsingJobData(notifyJobDataMap) // same data map as notify job
                        .Build();

                    ITrigger reminderTrigger = TriggerBuilder.Create()
                        .WithIdentity(evt.Id.ToString(), "eventReminders")
                        .StartAt(evt.ReminderTimestamp ?? DateTimeOffset.Now) // workaround for nullable timestamp
                        .ForJob(reminderJob)
                        .Build();

                    if (!await Scheduler.CheckExists(reminderTrigger.Key))
                    {
                        await Scheduler.ScheduleJob(reminderJob, reminderTrigger);
                    }
                }
            }

            if (evt.Repeat != RepeatType.None)
            {
                IJobDetail repeatJob = JobBuilder.Create<EventRepeatJob>()
                    .WithIdentity(evt.Id.ToString(), "eventRepeats")
                    .UsingJobData(new JobDataMap
                    {
                        ["eventId"] = evt.Id,
                        ["client"] = client,
                        ["channelId"] = channelId,
                        ["eventService"] = _eventService,
                        ["eventScheduler"] = this,
                        ["semaphore"] = semaphore
                    })
                    .Build();

                ITrigger repeatTrigger = TriggerBuilder.Create()
                    .WithIdentity(evt.Id.ToString(), "eventRepeats")
                    .StartAt(evt.EndTimestamp)
                    .ForJob(repeatJob)
                    .Build();

                if (!await Scheduler.CheckExists(repeatTrigger.Key))
                {
                    await Scheduler.ScheduleJob(repeatJob, repeatTrigger);
                }
            }
            else
            {
                var deleteJobDataMap = new JobDataMap
                {
                    ["client"] = client,
                    ["eventId"] = evt.Id,
                    ["eventService"] = _eventService,
                    ["semaphore"] = semaphore
                };

                IJobDetail deleteJob = JobBuilder.Create<EventDeleteJob>()
                    .WithIdentity(evt.Id.ToString(), "eventDeletions")
                    .UsingJobData(deleteJobDataMap)
                    .Build();

                ITrigger deleteTrigger = TriggerBuilder.Create()
                    .WithIdentity(evt.Id.ToString(), "eventDeletions")
                    .StartAt(evt.EndTimestamp)
                    .ForJob(deleteJob)
                    .Build();

                if (!await Scheduler.CheckExists(deleteTrigger.Key))
                {
                    await Scheduler.ScheduleJob(deleteJob, deleteTrigger);
                }
            }
        }

        public async Task UnscheduleEvent(Event evt)
        {
            await UnscheduleEvent(evt.Id);
        }

        public async Task UnscheduleEvent(EventViewModel evt)
        {
            await UnscheduleEvent(evt.Id);
        }

        public async Task UnscheduleEvent(Guid eventId)
        {
            await Scheduler.UnscheduleJob(new TriggerKey(eventId.ToString(), "eventNotifications"));
            await Scheduler.UnscheduleJob(new TriggerKey(eventId.ToString(), "eventReminders"));
            await Scheduler.UnscheduleJob(new TriggerKey(eventId.ToString(), "eventDeletions"));
            await Scheduler.UnscheduleJob(new TriggerKey(eventId.ToString(), "eventRepeats"));
        }

        public async Task RescheduleEvent(Event evt, DiscordClient client, ulong channelId)
        {
            await UnscheduleEvent(evt.Id);
            await ScheduleEvent(evt, client, channelId);
        }

        public async Task RescheduleEvent(EventViewModel evt, DiscordClient client, ulong channelId)
        {
            await UnscheduleEvent(evt.Id);
            await ScheduleEvent(evt, client, channelId);
        }
    }
}
