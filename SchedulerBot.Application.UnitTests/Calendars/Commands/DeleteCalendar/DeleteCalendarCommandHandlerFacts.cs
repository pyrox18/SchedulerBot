using MediatR;
using Moq;
using SchedulerBot.Application.Calendars.Commands.DeleteCalendar;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Calendars.Commands.DeleteCalendar
{
    public class DeleteCalendarCommandHandlerFacts
    {
        public class HandleDeleteCalendarCommandMethod
        {
            [Fact]
            public async Task ReturnsUnit()
            {
                var calendar = new Calendar
                {
                    Id = 1
                };

                var mockCalendarRepository = new Mock<ICalendarRepository>();
                mockCalendarRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar)
                    .Verifiable();
                mockCalendarRepository.Setup(x => x.DeleteAsync(It.IsAny<Calendar>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

                var command = new DeleteCalendarCommand
                {
                    CalendarId = calendar.Id
                };

                var handler = new DeleteCalendarCommandHandler(mockCalendarRepository.Object);
                var result = await handler.Handle(command);

                mockCalendarRepository.Verify();
                Assert.Equal(Unit.Value, result);
            }
        }
    }
}
