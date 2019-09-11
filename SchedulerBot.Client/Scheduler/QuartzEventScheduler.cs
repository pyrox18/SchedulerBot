using System;
using System.Threading;
using System.Threading.Tasks;
using Quartz;
using Quartz.Spi;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Client.Scheduler.Data;
using SchedulerBot.Client.Scheduler.Jobs;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Client.Scheduler
{
    public class QuartzEventScheduler : IEventScheduler
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;

        public IScheduler Scheduler { get; set; }

        public QuartzEventScheduler(ISchedulerFactory schedulerFactory, IJobFactory jobFactory)
        {
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            Scheduler.JobFactory = _jobFactory;

            // Schedule event polling job
            var pollingJobSchedule = new RecurringJobSchedule(new TimeSpan(1, 0, 0));
            var job = CreateJob(pollingJobSchedule);
            var trigger = CreateTrigger(pollingJobSchedule, job);

            await Scheduler.ScheduleJob(job, trigger);

            await Scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Scheduler?.Shutdown(cancellationToken);
        }

        public async Task RescheduleEvent(EventViewModel evt, int clientShardId, ulong channelId)
        {
            await UnscheduleEvent(evt.Id);
            await ScheduleEvent(evt, clientShardId, channelId);
        }

        public async Task ScheduleEvent(EventViewModel evt, int clientShardId, ulong channelId)
        {
            if (await Scheduler.CheckExists(new TriggerKey(evt.Id.ToString(), typeof(EventNotifyJob).FullName))
                || await Scheduler.CheckExists(new TriggerKey(evt.Id.ToString(), typeof(EventReminderJob).FullName))
                || (!evt.StartsInHours(2) && (evt.ReminderTimestamp == null || !evt.RemindInHours(2))))
            {
                return;
            }

            var data = new EventNotifyJobDataMap(clientShardId, channelId, evt.Id);

            if (!evt.HasStarted())
            {
                var schedule = new FixedJobSchedule(typeof(EventNotifyJob), data.JobDataMap, evt.StartTimestamp);

                var job = CreateJob(evt.Id, schedule);
                var trigger = CreateTrigger(evt.Id, schedule, job);

                await Scheduler.ScheduleJob(job, trigger);

                if (!(evt.ReminderTimestamp is null) && !evt.HasReminderPassed())
                {
                    // TODO: Replace DateTimeOffset.Now with abstracted interface
                    var reminderTimestamp = evt.ReminderTimestamp ?? DateTimeOffset.Now;
                    var reminderSchedule = new FixedJobSchedule(typeof(EventReminderJob), data.JobDataMap, reminderTimestamp);

                    var reminderJob = CreateJob(evt.Id, reminderSchedule);
                    var reminderTrigger = CreateTrigger(evt.Id, reminderSchedule, reminderJob);

                    await Scheduler.ScheduleJob(reminderJob, reminderTrigger);
                }
            }

            if (evt.Repeat != RepeatType.None)
            {
                // Schedule event repeat job
                var schedule = new FixedJobSchedule(typeof(EventRepeatJob), data.JobDataMap, evt.EndTimestamp);

                var job = CreateJob(evt.Id, schedule);
                var trigger = CreateTrigger(evt.Id, schedule, job);

                await Scheduler.ScheduleJob(job, trigger);
            }
            else
            {
                // Schedule event delete job
                var schedule = new FixedJobSchedule(typeof(EventDeleteJob), data.JobDataMap, evt.EndTimestamp);

                var job = CreateJob(evt.Id, schedule);
                var trigger = CreateTrigger(evt.Id, schedule, job);

                await Scheduler.ScheduleJob(job, trigger);
            }
        }

        public async Task UnscheduleEvent(EventViewModel evt)
        {
            await UnscheduleEvent(evt.Id);
        }

        public async Task UnscheduleEvent(Guid eventId)
        {
            await Scheduler.DeleteJob(new JobKey(eventId.ToString(), typeof(EventNotifyJob).FullName));
            await Scheduler.DeleteJob(new JobKey(eventId.ToString(), typeof(EventReminderJob).FullName));
            await Scheduler.DeleteJob(new JobKey(eventId.ToString(), typeof(EventRepeatJob).FullName));
            await Scheduler.DeleteJob(new JobKey(eventId.ToString(), typeof(EventDeleteJob).FullName));
        }

        private static IJobDetail CreateJob(Guid id, FixedJobSchedule schedule)
        {
            var jobType = schedule.JobType;

            return JobBuilder
                .Create(jobType)
                .WithIdentity(id.ToString(), jobType.FullName)
                .WithDescription(jobType.Name)
                .UsingJobData(schedule.JobDataMap)
                .Build();
        }

        private static ITrigger CreateTrigger(Guid id, FixedJobSchedule schedule, IJobDetail jobDetail)
        {
            var jobType = schedule.JobType;

            return TriggerBuilder
                .Create()
                .WithIdentity(id.ToString(), $"{jobType.FullName}.trigger")
                .StartAt(schedule.Timestamp)
                .ForJob(jobDetail)
                .Build();
        }

        private static IJobDetail CreateJob(RecurringJobSchedule schedule)
        {
            var jobType = schedule.JobType;

            return JobBuilder
                .Create(jobType)
                .WithIdentity(jobType.FullName)
                .WithDescription(jobType.Name)
                .UsingJobData(schedule.JobDataMap)
                .Build();
        }

        private static ITrigger CreateTrigger(RecurringJobSchedule schedule, IJobDetail jobDetail)
        {
            var jobType = schedule.JobType;

            return TriggerBuilder
                .Create()
                .WithIdentity($"{jobType.FullName}.trigger")
                .WithSimpleSchedule(x => x.WithInterval(schedule.Interval).RepeatForever())
                .ForJob(jobDetail)
                .Build();
        }
    }
}
