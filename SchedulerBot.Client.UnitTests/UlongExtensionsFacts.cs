using Xunit;
using SchedulerBot.Client.Extensions;

namespace SchedulerBot.Client.UnitTests
{
    public class UlongExtensionsFacts
    {
        public class AsChannelMentionMethod
        {
            [Fact]
            public void ReturnsChannelMentionString()
            {
                ulong channelId = 12345;

                var result = channelId.AsChannelMention();

                Assert.Equal("<#12345>", result);
            }

            [Fact]
            public void ReturnsUserMentionString()
            {
                ulong userId = 12345;

                var result = userId.AsUserMention();

                Assert.Equal("<@12345>", result);
            }

            [Fact]
            public void ReturnsRoleMentionString()
            {
                ulong roleId = 12345;

                var result = roleId.AsRoleMention();

                Assert.Equal("<@&12345>", result);
            }
        }
    }
}
