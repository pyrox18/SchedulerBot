using Moq;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Settings.Commands.ModifySetting;
using SchedulerBot.Data.Models;
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

                var command = new ModifyPrefixSettingCommand
                {
                    CalendarId = 1,
                    NewPrefix = "b"
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object);
                var result = await handler.Handle(command);

                Assert.Equal(command.NewPrefix, result.Prefix);
            }

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync((Calendar)null);

                var command = new ModifyPrefixSettingCommand
                {
                    CalendarId = 1,
                    NewPrefix = "b"
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object);
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

                var command = new ModifyDefaultChannelSettingCommand
                {
                    CalendarId = 1,
                    NewDefaultChannel = 3
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object);
                var result = await handler.Handle(command);

                Assert.Equal(command.NewDefaultChannel, result.DefaultChannel);
            }

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync((Calendar)null);

                var command = new ModifyDefaultChannelSettingCommand
                {
                    CalendarId = 1,
                    NewDefaultChannel = 3
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object);
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
                    Timezone = "a"
                };

                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);
                mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Calendar>()))
                    .Returns(Task.CompletedTask);

                var command = new ModifyTimezoneSettingCommand
                {
                    CalendarId = 1,
                    NewTimezone = "b"
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object);
                var result = await handler.Handle(command);

                Assert.Equal(command.NewTimezone, result.Timezone);
            }

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockRepository = new Mock<ICalendarRepository>();
                mockRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync((Calendar)null);

                var command = new ModifyTimezoneSettingCommand
                {
                    CalendarId = 1,
                    NewTimezone = "b"
                };

                var handler = new ModifySettingCommandHandler(mockRepository.Object);
                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(command));
            }
        }
    }
}
