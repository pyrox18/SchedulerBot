using Moq;
using SchedulerBot.Application.Events.Commands.ApplyEventRepeat;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Domain.Enumerations;
using SchedulerBot.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Events.Commands.ApplyEventRepeat
{
    public class ApplyEventRepeatCommandHandlerFacts
    {
        public class HandleApplyEventRepeatCommandMethod
        {
            [Fact]
            public async Task RepeatsDailyEventWithCorrectReminderTimestamp()
            {
                var @event = new Event
                {
                    Id = Guid.NewGuid(),
                    Calendar = new Calendar
                    {
                        Id = 1,
                        Timezone = "Asia/Kuala_Lumpur"
                    },
                    StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero),
                    ReminderTimestamp = new DateTimeOffset(2019, 1, 1, 11, 45, 0, TimeSpan.Zero),
                    Repeat = RepeatType.Daily,
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
                Assert.Equal(new DateTimeOffset(2019, 1, 2, 20, 0, 0, new TimeSpan(8, 0, 0)), @event.StartTimestamp);
                Assert.Equal(new DateTimeOffset(2019, 1, 2, 21, 0, 0, new TimeSpan(8, 0, 0)), @event.EndTimestamp);
                Assert.Equal(new DateTimeOffset(2019, 1, 2, 19, 45, 0, new TimeSpan(8, 0, 0)), @event.ReminderTimestamp);
            }
        }
    }
}
