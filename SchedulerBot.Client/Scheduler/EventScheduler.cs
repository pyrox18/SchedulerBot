using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using DSharpPlus;
using Quartz;
using Quartz.Impl;
using RedLockNet;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Scheduler
{
    public class EventScheduler : IEventScheduler
    {
        private readonly IEventService _eventService;
        private readonly IDistributedLockFactory _redlockFactory;
        public IScheduler Scheduler { get; set; }

        public EventScheduler(IEventService eventService, IDistributedLockFactory redlockFactory)
        {
            _eventService = eventService;
            _redlockFactory = redlockFactory;
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

        public async Task PollAndScheduleEvents(DiscordShardedClient shardedClient)
        {
            foreach (var sc in shardedClient.ShardClients)
            {
                var client = sc.Value;
                var calendarIds = client.Guilds.Keys;
                var events = await _eventService.GetEventsInHourIntervalAsync(2, calendarIds);

                foreach (var evt in events)
                {
                    await ScheduleEvent(evt, client, evt.Calendar.DefaultChannel);
                }
            }
        }

        public async Task ScheduleEvent(Event evt, DiscordClient client, ulong channelId)
        {
            if (await Scheduler.CheckExists(new TriggerKey(evt.Id.ToString(), "eventNotifications"))
                || await Scheduler.CheckExists(new TriggerKey(evt.Id.ToString(), "eventReminders"))
                || (!evt.StartsInHours(2) && (evt.ReminderTimestamp == null || !evt.RemindInHours(2))))
            {
                return;
            }

            var channel = await client.GetChannelAsync(channelId);
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


                await Scheduler.ScheduleJob(notifyJob, notifyTrigger);

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

                    await Scheduler.ScheduleJob(reminderJob, reminderTrigger);
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
                        ["redlockFactory"] = _redlockFactory
                    })
                    .Build();

                ITrigger repeatTrigger = TriggerBuilder.Create()
                    .WithIdentity(evt.Id.ToString(), "eventRepeats")
                    .StartAt(evt.EndTimestamp)
                    .ForJob(repeatJob)
                    .Build();

                await Scheduler.ScheduleJob(repeatJob, repeatTrigger);
            }
            else
            {
                var deleteJobDataMap = new JobDataMap
                {
                    ["client"] = client,
                    ["eventId"] = evt.Id,
                    ["channelId"] = channelId,
                    ["eventService"] = _eventService,
                    ["redlockFactory"] = _redlockFactory
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

                await Scheduler.ScheduleJob(deleteJob, deleteTrigger);
            }
        }

        public async Task UnscheduleEvent(Event evt)
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
    }
}
