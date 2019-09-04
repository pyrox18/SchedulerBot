using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SchedulerBot.Application.Events.Commands.ToggleEventRsvp;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Events.Commands.ToggleEventRsvp
{
    public class ToggleEventRsvpCommandHandlerFacts
    {
        public class HandleToggleEventRsvpCommandMethod
        {
            [Fact]
            public async Task ReturnsViewModelWithRsvpAdded()
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
                    RSVPs = new List<EventRSVP>()
                };

                calendar.Events.Add(@event);

                var mockCalendarRepository = new Mock<ICalendarRepository>();
                mockCalendarRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync(calendar);

                var mockEventRepository = new Mock<IEventRepository>();
                mockEventRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(calendar.Events);
                mockEventRepository.Setup(x => x.UpdateAsync(It.IsAny<Event>()))
                    .Returns(Task.CompletedTask);

                var mockDateTimeOffset = new Mock<IDateTimeOffset>();
                mockDateTimeOffset.Setup(x => x.Now)
                    .Returns(@event.StartTimestamp.AddDays(-1));

                var command = new ToggleEventRsvpCommand
                {
                    CalendarId = calendar.Id,
                    Index = 0,
                    UserId = 3
                };

                var handler = new ToggleEventRsvpCommandHandler(mockCalendarRepository.Object, mockEventRepository.Object, mockDateTimeOffset.Object);
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
                Assert.True(result.RsvpAdded);
            }

            [Fact]
            public async Task ReturnsViewModelWithRsvpRemoved()
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

                var mockEventRepository = new Mock<IEventRepository>();
                mockEventRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(calendar.Events);
                mockEventRepository.Setup(x => x.UpdateAsync(It.IsAny<Event>()))
                    .Returns(Task.CompletedTask);
                
                var mockDateTimeOffset = new Mock<IDateTimeOffset>();
                mockDateTimeOffset.Setup(x => x.Now)
                    .Returns(@event.StartTimestamp.AddDays(-1));

                var command = new ToggleEventRsvpCommand
                {
                    CalendarId = calendar.Id,
                    Index = 0,
                    UserId = 3
                };

                var handler = new ToggleEventRsvpCommandHandler(mockCalendarRepository.Object, mockEventRepository.Object, mockDateTimeOffset.Object);
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
                Assert.Empty(result.RSVPs);
                Assert.False(result.RsvpAdded);
            }

            [Fact]
            public async Task ThrowsWhenCalendarIsNull()
            {
                var mockCalendarRepository = new Mock<ICalendarRepository>();
                mockCalendarRepository.Setup(x => x.GetByIdAsync(It.IsAny<ulong>()))
                    .ReturnsAsync((Calendar)null);

                var mockEventRepository = new Mock<IEventRepository>();
                var mockDateTimeOffset = new Mock<IDateTimeOffset>();

                var command = new ToggleEventRsvpCommand
                {
                    CalendarId = 1,
                    Index = 0,
                    UserId = 2
                };

                var handler = new ToggleEventRsvpCommandHandler(mockCalendarRepository.Object, mockEventRepository.Object, mockDateTimeOffset.Object);

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

                var mockDateTimeOffset = new Mock<IDateTimeOffset>();

                var command = new ToggleEventRsvpCommand
                {
                    CalendarId = 1,
                    Index = 1,
                    UserId = 2
                };

                var handler = new ToggleEventRsvpCommandHandler(mockCalendarRepository.Object, mockEventRepository.Object, mockDateTimeOffset.Object);

                await Assert.ThrowsAsync<EventNotFoundException>(() => handler.Handle(command));
            }
        }
    }
}