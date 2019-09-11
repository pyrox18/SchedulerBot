using MediatR;
using Moq;
using SchedulerBot.Application.Calendars.Commands.CreateCalendar;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Calendars.Commands.CreateCalendar
{
    public class CreateCalendarCommandHandlerFacts
    {
        public class HandleCreateCalendarCommandMethod
        {
            [Fact]
            public async Task ReturnsUnit()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Prefix = "a"
                };

                var mockCalendarRepository = new Mock<ICalendarRepository>();
                mockCalendarRepository.Setup(x => x.AddAsync(It.IsAny<Calendar>()))
                    .ReturnsAsync(calendar)
                    .Verifiable();

                var command = new CreateCalendarCommand
                {
                    CalendarId = calendar.Id,
                    Prefix = calendar.Prefix
                };

                var handler = new CreateCalendarCommandHandler(mockCalendarRepository.Object);
                var result = await handler.Handle(command);

                mockCalendarRepository.Verify();
                Assert.Equal(Unit.Value, result);
            }
        }
    }
}
