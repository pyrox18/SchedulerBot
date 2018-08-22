namespace SchedulerBot.Client.Extensions
{
    public static class UlongExtensions
    {
        public static string AsChannelMention(this ulong channelId)
        {
            return $"<#{channelId}>";
        }

        public static string AsUserMention(this ulong userId)
        {
            return $"<@{userId}>";
        }

        public static string AsRoleMention(this ulong roleId)
        {
            return $"<@&{roleId}>";
        }
    }
}
