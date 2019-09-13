using MediatR;
using Moq;
using SchedulerBot.Application.Events.Commands.CleanPastEvents;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Events.Commands.CleanPastEvents
{
    public class CleanPastEventsCommandHandlerFacts
    {
        public class HandleCleanPastEventsCommandMethod
        {
            [Fact]
            public async Task ReturnsUnit()
            {
                var events = new List<Event>
                {
                    new Event
                    {
                        Id = Guid.NewGuid(),
                        StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero),
                        Repeat = RepeatType.None
                    },
                    new Event
                    {
                        Id = Guid.NewGuid(),
                        StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero),
                        Repeat = RepeatType.Daily
                    },
                    new Event
                    {
                        Id = Guid.NewGuid(),
                        StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero),
                        Repeat = RepeatType.Weekly
                    },
                    new Event
                    {
                        Id = Guid.NewGuid(),
                        StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero),
                        Repeat = RepeatType.Monthly
                    },
                    new Event
                    {
                        Id = Guid.NewGuid(),
                        StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero),
                        Repeat = RepeatType.MonthlyWeekday
                    }
                };

                var mockEventRepository = new Mock<IEventRepository>();
                mockEventRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(events);
                mockEventRepository.Setup(x => x.DeleteAsync(It.Is<Event>(e => e.Id == events[0].Id)))
                    .Returns(Task.CompletedTask)
                    .Verifiable();
                mockEventRepository.Setup(x => x.UpdateAsync(It.Is<Event>(e => e.Id == events[1].Id
                    && e.StartTimestamp == new DateTimeOffset(2019, 1, 2, 12, 0, 0, TimeSpan.Zero)
                    && e.EndTimestamp == new DateTimeOffset(2019, 1, 2, 13, 0, 0, TimeSpan.Zero))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();
                mockEventRepository.Setup(x => x.UpdateAsync(It.Is<Event>(e => e.Id == events[2].Id
                    && e.StartTimestamp == new DateTimeOffset(2019, 1, 8, 12, 0, 0, TimeSpan.Zero)
                    && e.EndTimestamp == new DateTimeOffset(2019, 1, 8, 13, 0, 0, TimeSpan.Zero))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();
                mockEventRepository.Setup(x => x.UpdateAsync(It.Is<Event>(e => e.Id == events[3].Id
                    && e.StartTimestamp == new DateTimeOffset(2019, 2, 1, 12, 0, 0, TimeSpan.Zero)
                    && e.EndTimestamp == new DateTimeOffset(2019, 2, 1, 13, 0, 0, TimeSpan.Zero))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();
                mockEventRepository.Setup(x => x.UpdateAsync(It.Is<Event>(e => e.Id == events[4].Id
                    && e.StartTimestamp == new DateTimeOffset(2019, 2, 5, 12, 0, 0, TimeSpan.Zero)
                    && e.EndTimestamp == new DateTimeOffset(2019, 2, 5, 13, 0, 0, TimeSpan.Zero))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

                var mockDateTimeOffset = new Mock<IDateTimeOffset>();
                mockDateTimeOffset.Setup(x => x.Now)
                    .Returns(new DateTimeOffset(2019, 1, 1, 14, 0, 0, TimeSpan.Zero));

                var command = new CleanPastEventsCommand();

                var handler = new CleanPastEventsCommandHandler(mockEventRepository.Object, mockDateTimeOffset.Object);
                var result = await handler.Handle(command);

                mockEventRepository.Verify();
                Assert.Equal(Unit.Value, result);
            }
        }
    }
}
