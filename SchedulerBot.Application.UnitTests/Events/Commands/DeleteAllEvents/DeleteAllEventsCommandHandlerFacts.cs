using System.Threading.Tasks;
using MediatR;
using Moq;
using SchedulerBot.Application.Events.Commands.DeleteAllEvents;
using SchedulerBot.Application.Interfaces;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Events.Commands.DeleteAllEvents
{
    public class DeleteAllEventsCommandHandlerFacts
    {
        public class HandleDeleteAllEventsCommandMethod
        {
            [Fact]
            public async Task ReturnsUnit()
            {
                var mockCalendarRepository = new Mock<ICalendarRepository>();
                mockCalendarRepository.Setup(x => x.DeleteAllEventsAsync(It.IsAny<ulong>()))
                    .Returns(Task.CompletedTask);

                var command = new DeleteAllEventsCommand
                {
                    CalendarId = 1
                };

                var handler = new DeleteAllEventsCommandHandler(mockCalendarRepository.Object);
                var result = await handler.Handle(command);

                Assert.Equal(Unit.Value, result);
            }
        }
    }
}