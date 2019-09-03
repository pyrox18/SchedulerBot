using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SchedulerBot.Application.Events.Queries.GetEvent;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Events.Queries.GetEvent
{
    public class GetEventQueryHandlerFacts
    {
        public class HandleGetEventByIndexQuery
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Events = new List<Event>()
                };

                var @event = new Event
                {
                    Calendar = calendar,
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

                calendar.Events.Add(@event);

                var mockRepository = new Mock<IEventRepository>();
                mockRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(calendar.Events);

                var query = new GetEventByIndexQuery
                {
                    CalendarId = 1,
                    Index = 0
                };

                var handler = new GetEventQueryHandler(mockRepository.Object);
                var result = await handler.Handle(query);

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

            [Fact]
            public async Task ThrowsWhenEventIndexOutOfBounds()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Events = new List<Event>()
                };

                var @event = new Event
                {
                    Calendar = calendar,
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

                calendar.Events.Add(@event);

                var mockRepository = new Mock<IEventRepository>();
                mockRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(calendar.Events);

                var query = new GetEventByIndexQuery
                {
                    CalendarId = 1,
                    Index = 1
                };

                var handler = new GetEventQueryHandler(mockRepository.Object);

                await Assert.ThrowsAsync<EventNotFoundException>(() => handler.Handle(query));
            }
        }
    }
}