using Microsoft.EntityFrameworkCore;
using Moq;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;
using SchedulerBot.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Persistence.UnitTests.Repositories
{
    public class EventRepositoryFacts
    {
        public class AddAsyncMethod
        {
            [Fact]
            public async Task AddsEvent()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(AddsEvent)}")
                    .Options;

                var @event = new Event
                {
                    Id = Guid.NewGuid()
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new EventRepository(context);

                    var result = await repository.AddAsync(@event);

                    Assert.Equal(@event.Id, result.Id);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Single(context.Events);
                }
            }
        }

        public class CountAsyncMethod
        {
            [Fact]
            public async Task ReturnsCount()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsCount)}")
                    .Options;

                var events = new List<Event>
                {
                    new Event
                    {
                        Id = Guid.NewGuid(),
                        Name = "a"
                    },
                    new Event
                    {
                        Id = Guid.NewGuid(),
                        Name = "a"
                    },
                    new Event
                    {
                        Id = Guid.NewGuid(),
                        Name = "b"
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Events.AddRangeAsync(events);
                    await context.SaveChangesAsync();
                }

                var mockSpecification = SetupBaseEventSpecification(new Mock<ISpecification<Event>>());
                mockSpecification.Setup(x => x.Criteria)
                    .Returns(c => c.Name == "a");

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new EventRepository(context);

                    var result = await repository.CountAsync(mockSpecification.Object);

                    Assert.Equal(2, result);
                }
            }
        }

        public class DeleteAsyncMethod
        {
            [Fact]
            public async Task DeletesEvent()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(DeletesEvent)}")
                    .Options;

                var @event = new Event
                {
                    Id = Guid.NewGuid()
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Events.AddAsync(@event);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new EventRepository(context);

                    await repository.DeleteAsync(@event);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Empty(context.Events);
                }
            }
        }

        public class GetByIdAsyncMethod
        {
            [Fact]
            public async Task ReturnsEvent()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsEvent)}")
                    .Options;

                var @event = new Event
                {
                    Id = Guid.NewGuid()
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Events.AddAsync(@event);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new EventRepository(context);

                    var result = await repository.GetByIdAsync(@event.Id);

                    Assert.Equal(@event.Id, result.Id);
                }
            }
        }

        public class ListAllAsyncMethod
        {
            [Fact]
            public async Task ReturnsEvents()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsEvents)}")
                    .Options;

                var events = new List<Event>
                {
                    new Event { Id = Guid.NewGuid() },
                    new Event { Id = Guid.NewGuid() }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Events.AddRangeAsync(events);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new EventRepository(context);

                    var result = await repository.ListAllAsync();

                    Assert.Equal(events.Count, result.Count);
                    foreach (var @event in events)
                    {
                        Assert.Contains(result, r => r.Id == @event.Id);
                    }
                }
            }
        }

        public class ListAsyncMethod
        {
            [Fact]
            public async Task ReturnsEvents()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsEvents)}")
                    .Options;

                var events = new List<Event>
                {
                    new Event
                    {
                        Id = Guid.NewGuid(),
                        Name = "a"
                    },
                    new Event
                    {
                        Id = Guid.NewGuid(),
                        Name = "a"
                    },
                    new Event
                    {
                        Id = Guid.NewGuid(),
                        Name = "b"
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Events.AddRangeAsync(events);
                    await context.SaveChangesAsync();
                }

                var mockSpecification = SetupBaseEventSpecification(new Mock<ISpecification<Event>>());
                mockSpecification.Setup(x => x.Criteria)
                    .Returns(c => c.Name == "a");

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new EventRepository(context);

                    var result = await repository.ListAsync(mockSpecification.Object);

                    Assert.Equal(2, result.Count);
                    for (int i = 0; i <= 1; i++)
                    {
                        Assert.Contains(result, x => x.Id == events[i].Id);
                    }
                }
            }
        }

        public class UpdateAsyncMethod
        {
            [Fact]
            public async Task UpdatesEvent()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(UpdatesEvent)}")
                    .Options;

                var @event = new Event
                {
                    Id = Guid.NewGuid(),
                    Name = "a"
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Events.AddAsync(@event);
                    await context.SaveChangesAsync();
                }

                var newName = "b";
                @event.Name = newName;

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new EventRepository(context);

                    await repository.UpdateAsync(@event);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var dbEvent = await context.Events.FirstAsync();

                    Assert.Equal(newName, dbEvent.Name);
                }
            }
        }

        public static Mock<ISpecification<Event>> SetupBaseEventSpecification(Mock<ISpecification<Event>> mock)
        {
            mock.Setup(x => x.Criteria)
                .Returns(_ => true);

            mock.Setup(x => x.Includes)
                .Returns(new List<Expression<Func<Event, object>>>());

            mock.Setup(x => x.IncludeStrings)
                .Returns(new List<string>());

            mock.Setup(x => x.OrderBy)
                .Returns(null);

            mock.Setup(x => x.OrderByDescending)
                .Returns(null);

            mock.Setup(x => x.IsPagingEnabled)
                .Returns(false);

            return mock;
        }
    }
}
