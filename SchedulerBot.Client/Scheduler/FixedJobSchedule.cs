using Quartz;
using System;

namespace SchedulerBot.Client.Scheduler
{
    public class FixedJobSchedule : BaseJobSchedule
    {
        public DateTimeOffset Timestamp { get; }

        public FixedJobSchedule(Type jobType, JobDataMap jobDataMap, DateTimeOffset timestamp) :
            base(jobType, jobDataMap)
        {
            Timestamp = timestamp;
        }
    }
}
