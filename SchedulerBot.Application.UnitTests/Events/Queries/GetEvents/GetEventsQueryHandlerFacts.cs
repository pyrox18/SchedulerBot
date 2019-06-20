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

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

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

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync((Calendar)null);

                var query = new GetEventsForCalendarQuery
                {
                    CalendarId = 1
                };

                var handler = new GetEventsQueryHandler(mockRepository.Object);

                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(query));
            }
        }
    }
}