using DSharpPlus.Entities;

namespace SchedulerBot.Client.Extensions
{
    public static class DiscordMemberExtensions
    {
        public static string GetUsernameAndDiscriminator(this DiscordMember member)
        {
            return member.Username + "#" + member.Discriminator;
        }
    }
}
