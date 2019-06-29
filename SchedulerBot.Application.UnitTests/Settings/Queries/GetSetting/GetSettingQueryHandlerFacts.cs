using System.Threading.Tasks;
using Moq;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Settings.Queries.GetSetting;
using SchedulerBot.Data.Models;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Settings.Queries.GetSetting
{
    public class GetSettingQueryHandlerFacts
    {
        public class HandleGetPrefixSettingQueryMethod
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Prefix = "a"
                };

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var query = new GetPrefixSettingQuery
                {
                    CalendarId = 1
                };

                var handler = new GetSettingQueryHandler(mockRepository.Object);
                var result = await handler.Handle(query);

                Assert.Equal(calendar.Prefix, result.Prefix);
            }

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync((Calendar)null);

                var query = new GetPrefixSettingQuery
                {
                    CalendarId = 1
                };

                var handler = new GetSettingQueryHandler(mockRepository.Object);
                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(query));
            }

            [Fact]
            public async Task ThrowsWhenPrefixIsEmpty()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Prefix = string.Empty
                };

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var query = new GetPrefixSettingQuery
                {
                    CalendarId = 1
                };

                var handler = new GetSettingQueryHandler(mockRepository.Object);
                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(query));
            }
        }
    }
}