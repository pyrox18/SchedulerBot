using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SchedulerBot.Application.Events.Commands.CreateEvent;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Domain.Models;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Events.Commands.CreateEvent
{
    public class CreateEventCommandHandlerFacts
    {
        public class HandleCreateEventCommandMethod
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

                var @event = new Event
                {
                    Calendar = new Calendar
                    {
                        Id = 1
                    },
                    Name = "Test Event",
                    Description = "Some description",
                    StartTimestamp = new DateTimeOffset(2019, 1, 1, 12, 0, 0, TimeSpan.Zero),
                    EndTimestamp = new DateTimeOffset(2019, 1, 1, 13, 0, 0, TimeSpan.Zero),
                    ReminderTimestamp = new DateTimeOffset(2019, 1, 1, 11, 0, 0, TimeSpan.Zero),
                    Repeat = RepeatType.None,
                    Mentions = new List<EventMention>
                    {
                        new EventMention
                        {
                            Id = Guid.NewGuid(),
                            TargetId = 2,
                            Type = MentionType.User
                        }
                    },
                    RSVPs = new List<EventRSVP>
                    {
                        new EventRSVP
                        {
                            Id = Guid.NewGuid(),
                            UserId = 3
                        }
                    }
                };

                var mockCalendarRepository = new Mock<ICalendarRepository>();
                mockCalendarRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var mockEventParser = new Mock<IEventParser>();
                mockEventParser.Setup(x => x.ParseNewEvent(It.IsAny<string[]>(), It.IsAny<string>()))
                    .Returns(@event);

                var mockEventRepository = new Mock<IEventRepository>();
                mockEventRepository.Setup(x => x.AddAsync(It.IsAny<Event>()))
                    .ReturnsAsync(@event);

                var command = new CreateEventCommand
                {
                    CalendarId = 1,
                    EventArgs = new[] { "Test", "Event", "9am" }
                };

                var handler = new CreateEventCommandHandler(mockCalendarRepository.Object, mockEventRepository.Object, mockEventParser.Object);
                var result = await handler.Handle(command);

                Assert.Equal(@event.Calendar.Id, result.CalendarId);
                Assert.Equal(@event.Name, result.Name);
                Assert.Equal(@event.Description, result.Description);
                Assert.Equal(@event.StartTimestamp, result.StartTimestamp);
                Assert.Equal(@event.EndTimestamp, result.EndTimestamp);
                Assert.Equal(@event.ReminderTimestamp, result.ReminderTimestamp);
                Assert.Equal(@event.Repeat, result.Repeat);
                Assert.Single(result.Mentions);
                Assert.Equal(@event.Mentions.First().TargetId, result.Mentions.First().TargetId);
                Assert.Single(result.RSVPs);
                Assert.Equal(@event.RSVPs.First().UserId, result.RSVPs.First().UserId);
            }

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockCalendarRepository = new Mock<ICalendarRepository>();
                mockCalendarRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(null as Calendar);

                var mockEventParser = new Mock<IEventParser>();
                var mockEventRepository = new Mock<IEventRepository>();

                var command = new CreateEventCommand
                {
                    CalendarId = 1,
                    EventArgs = new[] { "Test", "Event", "9am" }
                };

                var handler = new CreateEventCommandHandler(mockCalendarRepository.Object, mockEventRepository.Object, mockEventParser.Object);

                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(command));
            }

            [Fact]
            public async Task ThrowsWhenCalendarTimezoneIsEmpty()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Timezone = string.Empty
                };

                var mockCalendarRepository = new Mock<ICalendarRepository>();
                mockCalendarRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var mockEventParser = new Mock<IEventParser>();
                var mockEventRepository = new Mock<IEventRepository>();

                var command = new CreateEventCommand
                {
                    CalendarId = 1,
                    EventArgs = new[] { "Test", "Event", "9am" }
                };

                var handler = new CreateEventCommandHandler(mockCalendarRepository.Object, mockEventRepository.Object, mockEventParser.Object);

                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(command));
            }
        }
    }
}