using Quartz;
using System;

namespace SchedulerBot.Client.Scheduler.Data
{
    public class EventNotifyJobDataMap 
    {
        private const string _shardClientIdKey = "shardClientId";
        private const string _channelIdKey = "channelId";
        private const string _eventIdKey = "eventId";

        public JobDataMap JobDataMap { get; }

        public int ShardClientId
        {
            get
            {
                return JobDataMap.GetIntValue(_shardClientIdKey);
            }
            set
            {
                JobDataMap[_shardClientIdKey] = value;
            }
        }

        public ulong ChannelId
        {
            get
            {
                return (ulong)JobDataMap.Get(_channelIdKey);
            }
            set
            {
                JobDataMap[_channelIdKey] = value;
            }
        }

        public Guid EventId
        {
            get
            {
                return (Guid)JobDataMap.Get(_eventIdKey);
            }
            set
            {
                JobDataMap[_eventIdKey] = value;
            }
        }

        public EventNotifyJobDataMap(JobDataMap jobDataMap)
        {
            JobDataMap = jobDataMap;
        }

        public EventNotifyJobDataMap(int shardClientId, ulong channelId, Guid eventId)
        {
            JobDataMap = new JobDataMap();

            ShardClientId = shardClientId;
            ChannelId = channelId;
            EventId = eventId;
        }
    }
}
