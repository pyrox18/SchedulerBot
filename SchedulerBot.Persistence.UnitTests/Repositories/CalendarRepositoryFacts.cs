using Microsoft.EntityFrameworkCore;
using Moq;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Domain.Models;
using SchedulerBot.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Persistence.UnitTests.Repositories
{
    public class CalendarRepositoryFacts
    {
        public class AddAsyncMethod
        {
            [Fact]
            public async Task AddsCalendar()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(AddsCalendar)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new CalendarRepository(context);

                    var result = await repository.AddAsync(calendar);

                    Assert.Equal(calendar.Id, result.Id);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Single(context.Calendars);
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

                var calendars = new List<Calendar>
                {
                    new Calendar
                    {
                        Id = 1,
                        Prefix = "a"
                    },
                    new Calendar
                    {
                        Id = 2,
                        Prefix = "a"
                    },
                    new Calendar
                    {
                        Id = 3,
                        Prefix = "b"
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddRangeAsync(calendars);
                    await context.SaveChangesAsync();
                }

                var mockSpecification = SetupBaseCalendarSpecification(new Mock<ISpecification<Calendar>>());
                mockSpecification.Setup(x => x.Criteria)
                    .Returns(c => c.Prefix == "a");

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new CalendarRepository(context);

                    var result = await repository.CountAsync(mockSpecification.Object);

                    Assert.Equal(2, result);
                }
            }
        }

        public class DeleteAllEventsAsyncMethod
        {
            [Fact]
            public async Task DeletesAllEvents()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(DeletesAllEvents)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1,
                    Events = new List<Event>
                    {
                        new Event
                        {
                            Name = "Test Event",
                            Mentions = new List<EventMention>
                            {
                                new EventMention
                                {
                                    TargetId = 2,
                                    Type = MentionType.User
                                }
                            },
                            RSVPs = new List<EventRSVP>
                            {
                                new EventRSVP
                                {
                                    UserId = 2
                                }
                            }
                        }
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new CalendarRepository(context);

                    await repository.DeleteAllEventsAsync(calendar.Id);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Single(context.Calendars);
                    Assert.Empty(context.Events);
                    Assert.Empty(context.EventMentions);
                    Assert.Empty(context.EventRSVPs);
                }
            }
        }

        public class DeleteAsyncMethod
        {
            [Fact]
            public async Task DeletesCalendar()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(DeletesCalendar)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new CalendarRepository(context);

                    await repository.DeleteAsync(calendar);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Empty(context.Calendars);
                }
            }
        }

        public class GetByIdAsyncMethod
        {
            [Fact]
            public async Task ReturnsCalendarWithUlongId()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsCalendarWithUlongId)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new CalendarRepository(context);

                    var result = await repository.GetByIdAsync(calendar.Id);

                    Assert.Equal(calendar.Id, result.Id);
                }
            }
        }

        public class ListAllAsyncMethod
        {
            [Fact]
            public async Task ReturnsCalendars()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsCalendars)}")
                    .Options;

                var calendars = new List<Calendar>
                {
                    new Calendar { Id = 1 },
                    new Calendar { Id = 2 }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddRangeAsync(calendars);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new CalendarRepository(context);

                    var result = await repository.ListAllAsync();

                    Assert.Equal(calendars.Count, result.Count);
                    foreach (var calendar in calendars)
                    {
                        Assert.Contains(result, r => r.Id == calendar.Id);
                    }
                }
            }
        }

        public class ListAsyncMethod
        {
            [Fact]
            public async Task ReturnsCalendars()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsCalendars)}")
                    .Options;

                var calendars = new List<Calendar>
                {
                    new Calendar
                    {
                        Id = 1,
                        Prefix = "a"
                    },
                    new Calendar
                    {
                        Id = 2,
                        Prefix = "a"
                    },
                    new Calendar
                    {
                        Id = 3,
                        Prefix = "b"
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddRangeAsync(calendars);
                    await context.SaveChangesAsync();
                }

                var mockSpecification = SetupBaseCalendarSpecification(new Mock<ISpecification<Calendar>>());
                mockSpecification.Setup(x => x.Criteria)
                    .Returns(c => c.Prefix == "a");

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new CalendarRepository(context);

                    var result = await repository.ListAsync(mockSpecification.Object);

                    Assert.Equal(2, result.Count);
                    for (ulong i = 1; i <= 2; i++)
                    {
                        Assert.Contains(result, x => x.Id == i);
                    }
                }
            }
        }

        public class UpdateAsyncMethod
        {
            [Fact]
            public async Task UpdatesCalendar()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(UpdatesCalendar)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1,
                    Prefix = "a"
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.SaveChangesAsync();
                }

                var newPrefix = "b";
                calendar.Prefix = newPrefix;

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new CalendarRepository(context);

                    await repository.UpdateAsync(calendar);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var dbCalendar = await context.Calendars.FirstAsync();

                    Assert.Equal(newPrefix, dbCalendar.Prefix);
                }
            }
        }

        public static Mock<ISpecification<Calendar>> SetupBaseCalendarSpecification(Mock<ISpecification<Calendar>> mock)
        {
            mock.Setup(x => x.Criteria)
                .Returns(_ => true);

            mock.Setup(x => x.Includes)
                .Returns(new List<Expression<Func<Calendar, object>>>());

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
