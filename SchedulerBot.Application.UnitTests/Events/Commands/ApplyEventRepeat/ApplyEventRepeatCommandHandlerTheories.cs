using Moq;
using SchedulerBot.Application.Events.Commands.ApplyEventRepeat;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Events.Commands.ApplyEventRepeat
{
    public class ApplyEventRepeatCommandHandlerTheories
    {
        public class HandleApplyEventRepeatCommandMethod
        {
            public static IEnumerable<object[]> Data =>
                new List<object[]>
                {
                    new object[]
                    {
                        RepeatType.Daily,
                        new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2019, 1, 2, 12, 0, 0, TimeSpan.Zero)
                    },
                    new object[]
                    {
                        RepeatType.Weekly,
                        new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2019, 1, 8, 12, 0, 0, TimeSpan.Zero)
                    },
                    new object[]
                    {
                        RepeatType.Monthly,
                        new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2019, 2, 1, 12, 0, 0, TimeSpan.Zero)
                    },
                    new object[]
                    {
                        RepeatType.MonthlyWeekday,
                        new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                        new DateTimeOffset(2019, 2, 5, 12, 0, 0, TimeSpan.Zero)
                    },
                };

            [Theory]
            [MemberData(nameof(Data))]
            public async Task RepeatsEvent(RepeatType repeatType, DateTimeOffset initialStartTimestamp, DateTimeOffset expectedStartTimestamp)
            {
                var @event = new Event
                {
                    Id = Guid.NewGuid(),
                    Calendar = new Calendar
                    {
                        Id = 1,
                        Timezone = "Asia/Kuala_Lumpur"
                    },
                    StartTimestamp = initialStartTimestamp,
                    EndTimestamp = initialStartTimestamp.AddHours(1),
                    Repeat = repeatType,
                    Mentions = new List<EventMention>(),
                    RSVPs = new List<EventRSVP>()
                };

                var mockEventRepository = new Mock<IEventRepository>();
                mockEventRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                    .ReturnsAsync(@event);
                mockEventRepository.Setup(x => x.UpdateAsync(It.IsAny<Event>()))
                    .Returns(Task.CompletedTask);

                var command = new ApplyEventRepeatCommand
                {
                    EventId = @event.Id
                };

                var handler = new ApplyEventRepeatCommandHandler(mockEventRepository.Object);
                var result = await handler.Handle(command);

                Assert.Equal(@event.Id, result.Id);
                Assert.Equal(expectedStartTimestamp, @event.StartTimestamp);
                Assert.Equal(expectedStartTimestamp.AddHours(1), @event.EndTimestamp);
            }
        }
    }
}
