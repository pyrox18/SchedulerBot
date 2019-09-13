using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SchedulerBot.Application.Events.Queries.GetEvents;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Domain.Models;
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

                var mockDateTimeOffset = new Mock<IDateTimeOffset>();

                var query = new GetEventsForCalendarQuery
                {
                    CalendarId = calendar.Id
                };

                var handler = new GetEventsQueryHandler(mockRepository.Object, mockDateTimeOffset.Object);
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
        
        public class HandleGetEventsInIntervalForCalendarsQueryMethod
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var events = new List<Event>
                {
                    new Event
                    {
                        Calendar = new Calendar { Id = 1, DefaultChannel = 10 },
                        Id = Guid.NewGuid(),
                        Name = "Test Event 1",
                        StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero),
                        Mentions = new List<EventMention>(),
                        RSVPs = new List<EventRSVP>()
                    },
                    new Event
                    {
                        Calendar = new Calendar { Id = 2, DefaultChannel = 20 },
                        Id = Guid.NewGuid(),
                        Name = "Test Event 2",
                        StartTimestamp = new DateTimeOffset(2019, 1, 2, 12, 0, 0, TimeSpan.Zero),
                        EndTimestamp = new DateTimeOffset(2019, 1, 2, 13, 0, 0, TimeSpan.Zero),
                        Mentions = new List<EventMention>(),
                        RSVPs = new List<EventRSVP>()
                    }
                };

                var mockEventRepository = new Mock<IEventRepository>();
                mockEventRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(events);

                var mockDateTimeOffset = new Mock<IDateTimeOffset>();
                mockDateTimeOffset.Setup(x => x.Now)
                    .Returns(DateTimeOffset.Now);

                var query = new GetEventsInIntervalForCalendarsQuery
                {
                    CalendarIds = new List<ulong> { 1, 2 },
                    Interval = new TimeSpan(1, 0, 0)
                };

                var handler = new GetEventsQueryHandler(mockEventRepository.Object, mockDateTimeOffset.Object);
                var result = await handler.Handle(query);

                Assert.Equal(events.Count, result.Count);
                for (var i = 0; i < events.Count; i++)
                {
                    Assert.Equal(events[i].Id, result[i].Id);
                    Assert.Equal(events[i].Calendar.Id, result[i].CalendarId);
                    Assert.Equal(events[i].Calendar.DefaultChannel, result[i].DefaultChannel);
                    Assert.Equal(events[i].Name, result[i].Name);
                    Assert.Equal(events[i].StartTimestamp, result[i].StartTimestamp);
                    Assert.Equal(events[i].EndTimestamp, result[i].EndTimestamp);
                }
            }
        }
    }
}