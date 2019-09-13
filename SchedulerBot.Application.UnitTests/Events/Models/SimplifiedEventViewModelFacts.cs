using System;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Domain.Models;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Events.Models
{
    public class SimplifiedEventViewModelFacts
    {
        public class FromEventMethod
        {
            [Fact]
            public void ReturnsEventViewModel()
            {
                var @event = new Event
                {
                    Name = "Test Event",
                    StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero),
                };

                var result = SimplifiedEventViewModel.FromEvent(@event);

                Assert.Equal(@event.Name, result.Name);
                Assert.Equal(@event.StartTimestamp, result.StartTimestamp);
                Assert.Equal(@event.EndTimestamp, result.EndTimestamp);
            }
        }
    }
}