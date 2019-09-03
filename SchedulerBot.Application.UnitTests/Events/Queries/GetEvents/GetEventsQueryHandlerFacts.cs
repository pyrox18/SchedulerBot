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
                    Id = 1,
                    Events = new List<Event>
                    {
                        new Event
                        {
                            Id = Guid.NewGuid(),
                            Name = "Test Event 1",
                            StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                            EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero),
                        },
                        new Event
                        {
                            Id = Guid.NewGuid(),
                            Name = "Test Event 2",
                            StartTimestamp = new DateTimeOffset(2019, 1, 2, 12, 0, 0, TimeSpan.Zero),
                            EndTimestamp = new DateTimeOffset(2019, 1, 2, 13, 0, 0, TimeSpan.Zero),
                        }
                    }
                };

                var mockRepository = new Mock<IEventRepository>();
                mockRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(calendar.Events);

                var query = new GetEventsForCalendarQuery
                {
                    CalendarId = calendar.Id
                };

                var handler = new GetEventsQueryHandler(mockRepository.Object);
                var result = await handler.Handle(query);

                Assert.Equal(calendar.Events.Count, result.Count);
                for (var i = 0; i < calendar.Events.Count; i++)
                {
                    Assert.Equal(calendar.Events[i].Name, result[i].Name);
                    Assert.Equal(calendar.Events[i].StartTimestamp, result[i].StartTimestamp);
                    Assert.Equal(calendar.Events[i].EndTimestamp, result[i].EndTimestamp);
                }
            }
        }
    }
}