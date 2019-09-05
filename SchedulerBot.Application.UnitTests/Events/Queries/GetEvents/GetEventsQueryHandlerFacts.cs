using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SchedulerBot.Application.Events.Queries.GetEvents;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Events.Queries.GetEvents
{
    public class GetEventsQueryHandlerFacts
    {
        public class HandleGetEventsForCalendarQueryMethod
        {
            [Fact]
            public async Task ReturnsViewModelList()
            {
                var calendar = new Calendar
                {
                    Id = 1
                };

                var events = new List<Event>
                {
                    new Event
                    {
                        Calendar = calendar,
                        Id = Guid.NewGuid(),
                        Name = "Test Event 1",
                        StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero),
                        Mentions = new List<EventMention>(),
                        RSVPs = new List<EventRSVP>()
                    },
                    new Event
                    {
                        Calendar = calendar,
                        Id = Guid.NewGuid(),
                        Name = "Test Event 2",
                        StartTimestamp = new DateTimeOffset(2019, 1, 2, 12, 0, 0, TimeSpan.Zero),
                        EndTimestamp = new DateTimeOffset(2019, 1, 2, 13, 0, 0, TimeSpan.Zero),
                        Mentions = new List<EventMention>(),
                        RSVPs = new List<EventRSVP>()
                    }
                };

                var mockRepository = new Mock<IEventRepository>();
                mockRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(events);

                var query = new GetEventsForCalendarQuery
                {
                    CalendarId = calendar.Id
                };

                var handler = new GetEventsQueryHandler(mockRepository.Object);
                var result = await handler.Handle(query);

                Assert.Equal(events.Count, result.Count);
                for (var i = 0; i < events.Count; i++)
                {
                    Assert.Equal(events[i].Id, result[i].Id);
                    Assert.Equal(events[i].Name, result[i].Name);
                    Assert.Equal(events[i].StartTimestamp, result[i].StartTimestamp);
                    Assert.Equal(events[i].EndTimestamp, result[i].EndTimestamp);
                }
            }
        }
    }
}