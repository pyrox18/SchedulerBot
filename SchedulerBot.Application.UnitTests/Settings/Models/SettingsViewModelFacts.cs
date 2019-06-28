using SchedulerBot.Application.Settings.Models;
using SchedulerBot.Data.Models;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Settings.Models
{
    public class SettingsViewModelFacts
    {
        public class FromCalendarMethod
        {
            [Fact]
            public void ReturnsViewModel()
            {
                var calendar = new Calendar
                {
                    Prefix = "a",
                    DefaultChannel = 1,
                    Timezone = "b"
                };

                var result = SettingsViewModel.FromCalendar(calendar);

                Assert.Equal(calendar.Prefix, result.Prefix);
                Assert.Equal(calendar.DefaultChannel, result.DefaultChannel);
                Assert.Equal(calendar.Timezone, result.Timezone);
            }
        }
    }
}