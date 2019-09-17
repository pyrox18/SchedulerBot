using System;
using System.Collections.Generic;
using System.Linq;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Domain.Enumerations;
using SchedulerBot.Domain.Models;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Events.Models
{
    public class EventViewModelFacts
    {
        public class FromEventMethod
        {
            [Fact]
            public void ReturnsEventViewModel()
            {
                var @event = new Event
                {
                    Calendar = new Calendar
                    {
                        Id = 1
                    },
                    Name = "Test Event",
                    Description = "Some description",
                    StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero),
                    ReminderTimestamp = new DateTimeOffset(2019, 1, 1, 11, 0, 0, TimeSpan.Zero),
                    Repeat = RepeatType.None,
                    Mentions = new List<EventMention>
                    {
                        new EventMention
                        {
                            Id = Guid.NewGuid(),
                            TargetId = 2,
                            Type = MentionType.User
                        }
                    },
                    RSVPs = new List<EventRSVP>
                    {
                        new EventRSVP
                        {
                            Id = Guid.NewGuid(),
                            UserId = 3
                        }
                    }
                };

                var result = EventViewModel.FromEvent(@event);

                Assert.Equal(@event.Calendar.Id, result.CalendarId);
                Assert.Equal(@event.Name, result.Name);
                Assert.Equal(@event.Description, result.Description);
                Assert.Equal(@event.StartTimestamp, result.StartTimestamp);
                Assert.Equal(@event.EndTimestamp, result.EndTimestamp);
                Assert.Equal(@event.ReminderTimestamp, result.ReminderTimestamp);
                Assert.Equal(@event.Repeat, result.Repeat);
                Assert.Single(result.Mentions);
                Assert.Equal(@event.Mentions.First().TargetId, result.Mentions.First().TargetId);
                Assert.Single(result.RSVPs);
                Assert.Equal(@event.RSVPs.First().UserId, result.RSVPs.First().UserId);
            }
        }
    }
}