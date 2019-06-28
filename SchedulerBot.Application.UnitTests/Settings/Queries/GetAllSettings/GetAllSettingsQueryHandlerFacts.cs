using System.Threading.Tasks;
using Moq;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Settings.Queries.GetAllSettings;
using SchedulerBot.Data.Models;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Settings.Queries.GetAllSettings
{
    public class GetAllSettingsQueryHandlerFacts
    {
        public class HandleGetAllSettingsQueryMethod
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Prefix = "a",
                    DefaultChannel = 2,
                    Timezone = "b"
                };

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var query = new GetAllSettingsQuery
                {
                    CalendarId = 1
                };

                var handler = new GetAllSettingsQueryHandler(mockRepository.Object);
                var result = await handler.Handle(query);

                Assert.Equal(calendar.Prefix, result.Prefix);
                Assert.Equal(calendar.DefaultChannel, result.DefaultChannel);
                Assert.Equal(calendar.Timezone, result.Timezone);
            }

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync((Calendar)null);

                var query = new GetAllSettingsQuery
                {
                    CalendarId = 1
                };

                var handler = new GetAllSettingsQueryHandler(mockRepository.Object);
                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(query));
            }

            [Fact]
            public async Task ThrowsWhenTimezoneIsEmpty()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Prefix = "a",
                    DefaultChannel = 2,
                    Timezone = string.Empty
                };

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var query = new GetAllSettingsQuery
                {
                    CalendarId = 1
                };

                var handler = new GetAllSettingsQueryHandler(mockRepository.Object);
                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(query));
            }
        }
    }
}