using System.Threading.Tasks;
using Moq;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Settings.Queries.GetSetting;
using SchedulerBot.Domain.Models;
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

        public class HandleGetDefaultChannelSettingQueryMethod
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    DefaultChannel = 2
                };

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var query = new GetDefaultChannelSettingQuery
                {
                    CalendarId = 1
                };

                var handler = new GetSettingQueryHandler(mockRepository.Object);
                var result = await handler.Handle(query);

                Assert.Equal(calendar.DefaultChannel, result.DefaultChannel);
            }

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync((Calendar)null);

                var query = new GetDefaultChannelSettingQuery
                {
                    CalendarId = 1
                };

                var handler = new GetSettingQueryHandler(mockRepository.Object);
                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(query));
            }

            [Fact]
            public async Task ThrowsWhenDefaultChannelIsDefault()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    DefaultChannel = default
                };

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var query = new GetDefaultChannelSettingQuery
                {
                    CalendarId = 1
                };

                var handler = new GetSettingQueryHandler(mockRepository.Object);
                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(query));
            }
        }

        public class HandleGetTimezoneSettingQueryMethod
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Timezone = "a"
                };

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var query = new GetTimezoneSettingQuery
                {
                    CalendarId = 1
                };

                var handler = new GetSettingQueryHandler(mockRepository.Object);
                var result = await handler.Handle(query);

                Assert.Equal(calendar.Timezone, result.Timezone);
            }

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync((Calendar)null);

                var query = new GetTimezoneSettingQuery
                {
                    CalendarId = 1
                };

                var handler = new GetSettingQueryHandler(mockRepository.Object);
                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(query));
            }

            [Fact]
            public async Task ThrowsWhenTimezoneIsEmpty()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Timezone = string.Empty
                };

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var query = new GetTimezoneSettingQuery
                {
                    CalendarId = 1
                };

                var handler = new GetSettingQueryHandler(mockRepository.Object);
                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(query));
            }
        }
    }
}