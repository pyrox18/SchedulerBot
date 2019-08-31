using Microsoft.EntityFrameworkCore;
using SchedulerBot.Data.Models;
using SchedulerBot.Persistence.Repositories;
using System;
using System.Collections.Generic;
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
    }
}
