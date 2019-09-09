using Quartz;
using System;

namespace SchedulerBot.Client.Scheduler.Data
{
    public class EventNotifyJobDataMap : JobDataMap
    {
        private const string _shardClientIdKey = "shardClientId";
        private const string _channelIdKey = "channelId";
        private const string _eventIdKey = "eventId";

        public int ShardClientId
        {
            get
            {
                return GetIntValue(_shardClientIdKey);
            }
        }

        public ulong ChannelId
        {
            get
            {
                return (ulong)Get(_channelIdKey);
            }
        }

        public Guid EventId
        {
            get
            {
                return (Guid)Get(_eventIdKey);
            }
        }

        public EventNotifyJobDataMap(int shardClientId, ulong channelId, Guid eventId)
        {
            Put(_shardClientIdKey, shardClientId);
            Put(_channelIdKey, channelId);
            Put(_eventIdKey, eventId);
        }
    }
}
