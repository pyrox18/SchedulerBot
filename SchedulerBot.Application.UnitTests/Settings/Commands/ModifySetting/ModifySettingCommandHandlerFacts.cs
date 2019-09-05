using Moq;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Settings.Commands.ModifySetting;
using SchedulerBot.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Settings.Commands.ModifySetting
{
    public class ModifySettingCommandHandlerFacts
    {
        public class HandleModifyPrefixSettingCommandMethod
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
                mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
                    .Returns(Task.CompletedTask);

                var mockEventRepository = new Mock<IEventRepository>();
                var mockDateTimeOffset = new Mock<IDateTimeOffset>();

                var command = new ModifyPrefixSettingCommand
                {
                    CalendarId = 1,
                    NewPrefix = "b"
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object, mockEventRepository.Object, mockDateTimeOffset.Object);
                var result = await handler.Handle(command);

                Assert.Equal(command.NewPrefix, result.Prefix);
            }

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync((Calendar)null);

                var mockEventRepository = new Mock<IEventRepository>();
                var mockDateTimeOffset = new Mock<IDateTimeOffset>();

                var command = new ModifyPrefixSettingCommand
                {
                    CalendarId = 1,
                    NewPrefix = "b"
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object, mockEventRepository.Object, mockDateTimeOffset.Object);
                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(command));
            }
        }

        public class HandleModifyDefaultChannelSettingCommandMethod
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
                mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
                    .Returns(Task.CompletedTask);

                var mockEventRepository = new Mock<IEventRepository>();
                var mockDateTimeOffset = new Mock<IDateTimeOffset>();

                var command = new ModifyDefaultChannelSettingCommand
                {
                    CalendarId = 1,
                    NewDefaultChannel = 3
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object, mockEventRepository.Object, mockDateTimeOffset.Object);
                var result = await handler.Handle(command);

                Assert.Equal(command.NewDefaultChannel, result.DefaultChannel);
            }

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync((Calendar)null);

                var mockEventRepository = new Mock<IEventRepository>();
                var mockDateTimeOffset = new Mock<IDateTimeOffset>();

                var command = new ModifyDefaultChannelSettingCommand
                {
                    CalendarId = 1,
                    NewDefaultChannel = 3
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object, mockEventRepository.Object, mockDateTimeOffset.Object);
                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(command));
            }
        }

        public class HandleModifyTimezoneSettingCommandMethod
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Timezone = "Asia/Kuala_Lumpur",
                    Events = new List<Event>
                    {
                        new Event
                        {
                            StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                            EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero)
                        }
                    }
                };

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);
                mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
                    .Returns(Task.CompletedTask);

                var mockEventRepository = new Mock<IEventRepository>();
                mockEventRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(calendar.Events);

                var mockDateTimeOffset = new Mock<IDateTimeOffset>();
                mockDateTimeOffset.Setup(x => x.Now)
                    .Returns(calendar.Events[0].StartTimestamp.AddDays(-1));

                var command = new ModifyTimezoneSettingCommand
                {
                    CalendarId = 1,
                    NewTimezone = "Asia/Tokyo"
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object, mockEventRepository.Object, mockDateTimeOffset.Object);
                var result = await handler.Handle(command);

                Assert.Equal(command.NewTimezone, result.Timezone);
            }

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync((Calendar)null);

                var mockEventRepository = new Mock<IEventRepository>();
                var mockDateTimeOffset = new Mock<IDateTimeOffset>();

                var command = new ModifyTimezoneSettingCommand
                {
                    CalendarId = 1,
                    NewTimezone = "Asia/Tokyo"
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object, mockEventRepository.Object, mockDateTimeOffset.Object);
                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(command));
            }

            [Fact]
            public async Task ThrowsWhenStartedEventsExistInNewTimezone()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Timezone = "Asia/Kuala_Lumpur",
                    Events = new List<Event>
                    {
                        new Event
                        {
                            StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                            EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero)
                        }
                    }
                };

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);
                mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
                    .Returns(Task.CompletedTask);

                var mockEventRepository = new Mock<IEventRepository>();
                mockEventRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(calendar.Events);

                var mockDateTimeOffset = new Mock<IDateTimeOffset>();
                mockDateTimeOffset.Setup(x => x.Now)
                    .Returns(calendar.Events[0].StartTimestamp.AddMinutes(-1));

                var command = new ModifyTimezoneSettingCommand
                {
                    CalendarId = 1,
                    NewTimezone = "Asia/Tokyo"
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object, mockEventRepository.Object, mockDateTimeOffset.Object);

                await Assert.ThrowsAsync<EventStartInNewTimezonePastException>(() => handler.Handle(command));
            }
        }
    }
}
