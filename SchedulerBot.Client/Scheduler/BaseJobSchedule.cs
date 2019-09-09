using Quartz;
using System;

namespace SchedulerBot.Client.Scheduler
{
    public class BaseJobSchedule
    {
        public Type JobType { get; }
        public JobDataMap JobDataMap { get; }

        public BaseJobSchedule(Type jobType, JobDataMap jobDataMap)
        {
            JobType = jobType;
            JobDataMap = jobDataMap;
        }
    }
}
