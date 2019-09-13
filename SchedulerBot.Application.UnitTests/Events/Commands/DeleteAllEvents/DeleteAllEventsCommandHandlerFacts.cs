using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Moq;
using SchedulerBot.Application.Events.Commands.DeleteAllEvents;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Domain.Models;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Events.Commands.DeleteAllEvents
{
    public class DeleteAllEventsCommandHandlerFacts
    {
        public class HandleDeleteAllEventsCommandMethod
        {
            [Fact]
            public async Task ReturnsUnit()
            {
                var events = new List<Event>
                {
                    new Event
                    {
                        Id = Guid.NewGuid()
                    },
                    new Event
                    {
                        Id = Guid.NewGuid()
                    },
                    new Event
                    {
                        Id = Guid.NewGuid()
                    }
                };

                var mockCalendarRepository = new Mock<ICalendarRepository>();
                mockCalendarRepository.Setup(x => x.DeleteAllEventsAsync(It.IsAny<ulong>()))
                    .Returns(Task.CompletedTask);

                var mockEventRepository = new Mock<IEventRepository>();
                mockEventRepository.Setup(x => x.ListAsync(It.IsAny<ISpecification<Event>>()))
                    .ReturnsAsync(events);

                var command = new DeleteAllEventsCommand
                {
                    CalendarId = 1
                };

                var handler = new DeleteAllEventsCommandHandler(mockCalendarRepository.Object, mockEventRepository.Object);
                var result = await handler.Handle(command);

                Assert.Equal(events.Count, result.EventIds.Count);
                foreach (var id in events.Select(e => e.Id))
                {
                    Assert.Contains(result.EventIds, r => r == id);
                }
            }
        }
    }
}