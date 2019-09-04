using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SchedulerBot.Application.Events.Commands.UpdateEvent;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Events.Commands.UpdateEvent
{
    public class UpdateEventCommandHandlerFacts
    {
        public class HandleUpdateEventCommandMethod
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    DefaultChannel = 2,
                    Prefix = "+",
                    Timezone = "Asia/Kuala_Lumpur",
                    Events = new List<Event>()
                };

                var @event = new Event
                {
                    Calendar = calendar,
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

                calendar.Events.Add(@event);

                var mockCalendarRepository = new Mock<ICalendarRepository>();
                mockCalendarRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var mockEventParser = new Mock<IEventParser>();
                mockEventParser.Setup(x => x.ParseUpdateEvent(It.IsAny<Event>(), It.IsAny<string[]>(), It.IsAny<string>()))
                    .Returns(@event);

                var mockEventRepository = new Mock<IEventRepository>();
                mockEventRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(calendar.Events);
                mockEventRepository.Setup(x => x.UpdateAsync(It.IsAny<Event>()))
                    .Returns(Task.CompletedTask);

                var mockDateTimeOffset = new Mock<IDateTimeOffset>();
                mockDateTimeOffset.Setup(x => x.Now)
                    .Returns(@event.StartTimestamp.AddDays(-1));

                var command = new UpdateEventCommand
                {
                    CalendarId = 1,
                    EventIndex = 0,
                    EventArgs = new[] { "Test", "Event", "9am" }
                };

                var handler = new UpdateEventCommandHandler(mockCalendarRepository.Object, mockEventParser.Object, mockEventRepository.Object, mockDateTimeOffset.Object);
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
                    .ReturnsAsync((Calendar)null);

                var mockEventRepository = new Mock<IEventRepository>();
                var mockEventParser = new Mock<IEventParser>();
                var mockDateTimeOffset = new Mock<IDateTimeOffset>();

                var command = new UpdateEventCommand
                {
                    CalendarId = 1,
                    EventIndex = 0,
                    EventArgs = new[] {"Test", "Event", "9am"}
                };

                var handler = new UpdateEventCommandHandler(mockCalendarRepository.Object, mockEventParser.Object, mockEventRepository.Object, mockDateTimeOffset.Object);

                await Assert.ThrowsAsync<CalendarNotInitialisedException>(() => handler.Handle(command));
            }

            [Fact]
            public async Task ThrowsWhenEventIndexOutOfBounds()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Events = new List<Event>()
                };

                var @event = new Event
                {
                    Calendar = calendar,
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

                calendar.Events.Add(@event);

                var mockCalendarRepository = new Mock<ICalendarRepository>();
                mockCalendarRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var mockEventRepository = new Mock<IEventRepository>();
                mockEventRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(calendar.Events);

                var mockEventParser = new Mock<IEventParser>();
                var mockDateTimeOffset = new Mock<IDateTimeOffset>();

                var command = new UpdateEventCommand
                {
                    CalendarId = 1,
                    EventIndex = 1,
                    EventArgs = new []{"Test", "Event", "9am"}
                };

                var handler = new UpdateEventCommandHandler(mockCalendarRepository.Object, mockEventParser.Object, mockEventRepository.Object, mockDateTimeOffset.Object);

                await Assert.ThrowsAsync<EventNotFoundException>(() => handler.Handle(command));
            }

            [Fact]
            public async Task ThrowsWhenEventAlreadyStarted()
            {
                var calendar = new Calendar
                {
                    Id = 1,
                    Events = new List<Event>()
                };

                var @event = new Event
                {
                    Calendar = calendar,
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

                calendar.Events.Add(@event);

                var mockCalendarRepository = new Mock<ICalendarRepository>();
                mockCalendarRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var mockEventRepository = new Mock<IEventRepository>();
                mockEventRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(calendar.Events);

                var mockEventParser = new Mock<IEventParser>();
                var mockDateTimeOffset = new Mock<IDateTimeOffset>();
                mockDateTimeOffset.Setup(x => x.Now)
                    .Returns(@event.StartTimestamp.AddDays(1));

                var command = new UpdateEventCommand
                {
                    CalendarId = 1,
                    EventIndex = 0,
                    EventArgs = new []{"Test", "Event", "9am"}
                };

                var handler = new UpdateEventCommandHandler(mockCalendarRepository.Object, mockEventParser.Object, mockEventRepository.Object, mockDateTimeOffset.Object);

                await Assert.ThrowsAsync<EventAlreadyStartedException>(() => handler.Handle(command));
            }
        }
    }
}