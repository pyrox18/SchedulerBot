using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerBot.Client.Extensions
{
    public static class UlongExtensions
    {
        public static string AsChannelMention(this ulong channelId)
        {
            return $"<#{channelId}>";
        }
    }
}
