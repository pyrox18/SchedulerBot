using System;
using Quartz;
using SchedulerBot.Client.Scheduler.Jobs;

namespace SchedulerBot.Client.Scheduler
{
    public class RecurringJobSchedule : BaseJobSchedule
    {
        public TimeSpan Interval { get; }

        public RecurringJobSchedule(TimeSpan interval) :
            base(typeof(EventPollingJob), new JobDataMap())
        {
            Interval = interval;
        }
    }
}
