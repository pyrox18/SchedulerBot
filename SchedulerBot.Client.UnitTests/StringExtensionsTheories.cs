using Xunit;
using SchedulerBot.Client.Extensions;

namespace SchedulerBot.Client.UnitTests
{
    public class StringExtensionsTheories
    {
        public class RemoveCaseInsensitiveMethod
        {
            [Theory]
            [InlineData("aBcDeFGhIj", "ghij", "aBcDeF")]
            [InlineData("asdFGHjKl", "ASD", "FGHjKl")]
            [InlineData("sAlMon", "alm", "son")]
            [InlineData("ababababa", "A", "bbbb")]
            public void ReturnsStringWithRemovedPart(string source, string partToRemove, string expected)
            {
                var result = source.RemoveCaseInsensitive(partToRemove);

                Assert.Equal(expected, result);
            }
        }
    }
}
