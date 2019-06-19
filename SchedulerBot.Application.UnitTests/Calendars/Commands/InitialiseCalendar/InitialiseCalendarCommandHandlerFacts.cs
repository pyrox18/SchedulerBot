using System.Threading.Tasks;
using Moq;
using SchedulerBot.Application.Calendars.Commands.InitialiseCalendar;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Calendars.Commands.InitialiseCalendar
{
    public class InitialiseCalendarCommandHandlerFacts
    {
        public class HandleInitialiseCalendarCommandMethod
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    DefaultChannel = 2,
                    Prefix = "+",
                    Timezone = "Asia/Kuala_Lumpur"
                };

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.AddAsync(It.IsAny<Calendar>()))
                    .ReturnsAsync(calendar);

                var command = new InitialiseCalendarCommand
                {
                    CalendarId = calendar.Id,
                    ChannelId = calendar.DefaultChannel,
                    Prefix = calendar.Prefix,
                    Timezone = calendar.Timezone
                };

                var handler = new InitialiseCalendarCommandHandler(mockRepository.Object);
                var result = await handler.Handle(command);

                Assert.Equal(calendar.Id, result.CalendarId);
                Assert.Equal(calendar.DefaultChannel, result.ChannelId);
                Assert.Equal(calendar.Timezone, result.Timezone);
            }
        }
    }
}