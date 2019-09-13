using Microsoft.EntityFrameworkCore;
using Moq;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Domain.Enumerations;
using SchedulerBot.Domain.Models;
using SchedulerBot.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Persistence.UnitTests.Repositories
{
    public class PermissionRepositoryFacts
    {
        public class AddAsyncMethod
        {
            [Fact]
            public async Task AddsPermission()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(AddsPermission)}")
                    .Options;

                var permission = new Permission
                {
                    Id = Guid.NewGuid()
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.AddAsync(permission);

                    Assert.Equal(permission.Id, result.Id);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Single(context.Permissions);
                }
            }
        }

        public class AllowRolePermissionAsync
        {
            [Fact]
            public async Task AllowsPermissionForRole()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(AllowsPermissionForRole)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                ulong roleId = 2;
                var node = PermissionNode.EventCreate;

                var permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    Calendar = calendar,
                    TargetId = roleId,
                    Type = PermissionType.Role,
                    Node = node
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.Permissions.AddAsync(permission);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    await repository.AllowRolePermissionAsync(calendar.Id, roleId, node);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Empty(context.Permissions);
                }
            }

            [Fact]
            public async Task AllowsPermissionForEveryoneRole()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(AllowsPermissionForEveryoneRole)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                ulong roleId = 1;
                var node = PermissionNode.EventCreate;

                var permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    Calendar = calendar,
                    TargetId = roleId,
                    Type = PermissionType.Everyone,
                    Node = node
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.Permissions.AddAsync(permission);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    await repository.AllowRolePermissionAsync(calendar.Id, roleId, node);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Empty(context.Permissions);
                }
            }

            [Fact]
            public async Task DoesNothingWhenPermissionAlreadyAllowed()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(DoesNothingWhenPermissionAlreadyAllowed)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                ulong roleId = 2;
                var node = PermissionNode.EventCreate;

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    await repository.AllowRolePermissionAsync(calendar.Id, roleId, node);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Empty(context.Permissions);
                }
            }
        }

        public class AllowUserPermissionAsync
        {
            [Fact]
            public async Task AllowsPermissionForUser()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(AllowsPermissionForUser)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                ulong userId = 2;
                var node = PermissionNode.EventCreate;

                var permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    Calendar = calendar,
                    TargetId = userId,
                    Type = PermissionType.User,
                    Node = node
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.Permissions.AddAsync(permission);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    await repository.AllowUserPermissionAsync(calendar.Id, userId, node);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Empty(context.Permissions);
                }
            }

            [Fact]
            public async Task DoesNothingWhenPermissionAlreadyAllowed()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(DoesNothingWhenPermissionAlreadyAllowed)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                ulong userId = 2;
                var node = PermissionNode.EventCreate;

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    await repository.AllowUserPermissionAsync(calendar.Id, userId, node);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Empty(context.Permissions);
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

                var permissions = new List<Permission>
                {
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Node = PermissionNode.EventCreate
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Node = PermissionNode.EventCreate
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Node = PermissionNode.EventDelete
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Permissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                }

                var mockSpecification = SetupBasePermissionSpecification(new Mock<ISpecification<Permission>>());
                mockSpecification.Setup(x => x.Criteria)
                    .Returns(c => c.Node == PermissionNode.EventCreate);

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.CountAsync(mockSpecification.Object);

                    Assert.Equal(2, result);
                }
            }
        }

        public class CheckPermissionAsyncMethod
        {
            [Fact]
            public async Task ReturnsTrueForUserMatch()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsTrueForUserMatch)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1,
                };

                var node = PermissionNode.EventCreate;

                List<Permission> permissions = new List<Permission>
                {
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node,
                        TargetId = 2,
                        Type = PermissionType.User,
                        IsDenied = true
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node,
                        TargetId = 3,
                        Type = PermissionType.Role,
                        IsDenied = true
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.Permissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.CheckPermissionAsync(calendar.Id, node, 2, new List<ulong> { 1 });

                    Assert.True(result);
                }
            }

            [Fact]
            public async Task ReturnsTrueForRoleMatch()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsTrueForRoleMatch)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1,
                };

                var node = PermissionNode.EventCreate;

                List<Permission> permissions = new List<Permission>
                {
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node,
                        TargetId = 2,
                        Type = PermissionType.User,
                        IsDenied = true
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node,
                        TargetId = 3,
                        Type = PermissionType.Role,
                        IsDenied = true
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.Permissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.CheckPermissionAsync(calendar.Id, node, 4, new List<ulong> { 1, 3 });

                    Assert.True(result);
                }
            }

            [Fact]
            public async Task ReturnsTrueForEveryoneRoleMatch()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsTrueForEveryoneRoleMatch)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1,
                };

                var node = PermissionNode.EventCreate;

                List<Permission> permissions = new List<Permission>
                {
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node,
                        TargetId = 2,
                        Type = PermissionType.User,
                        IsDenied = true
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node,
                        TargetId = 3,
                        Type = PermissionType.Role,
                        IsDenied = true
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node,
                        TargetId = 1,
                        Type = PermissionType.Everyone,
                        IsDenied = true
                    },
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.Permissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.CheckPermissionAsync(calendar.Id, node, 4, new List<ulong>());

                    Assert.True(result);
                }
            }

            [Fact]
            public async Task ReturnsFalse()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsFalse)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1,
                };

                var node = PermissionNode.EventCreate;

                List<Permission> permissions = new List<Permission>
                {
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node,
                        TargetId = 2,
                        Type = PermissionType.User,
                        IsDenied = true
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node,
                        TargetId = 3,
                        Type = PermissionType.Role,
                        IsDenied = true
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.Permissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.CheckPermissionAsync(calendar.Id, node, 4, new List<ulong> { 1 });

                    Assert.False(result);
                }
            }
        }

        public class DeleteAsyncMethod
        {
            [Fact]
            public async Task DeletesPermission()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(DeletesPermission)}")
                    .Options;

                var permission = new Permission
                {
                    Id = Guid.NewGuid()
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Permissions.AddAsync(permission);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    await repository.DeleteAsync(permission);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Empty(context.Permissions);
                }
            }
        }

        public class DenyRolePermissionAsyncMethod
        {
            [Fact]
            public async Task DeniesPermissionForRole()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(DeniesPermissionForRole)}")
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

                ulong roleId = 2;
                var node = PermissionNode.EventCreate;

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    await repository.DenyRolePermissionAsync(calendar.Id, roleId, node);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Single(context.Permissions);

                    var dbPermission = await context.Permissions
                        .Include(p => p.Calendar)
                        .FirstAsync();

                    Assert.Equal(calendar.Id, dbPermission.Calendar.Id);
                    Assert.Equal(roleId, dbPermission.TargetId);
                    Assert.Equal(PermissionType.Role, dbPermission.Type);
                    Assert.Equal(node, dbPermission.Node);
                }
            }

            [Fact]
            public async Task DeniesPermissionForEveryoneRole()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(DeniesPermissionForEveryoneRole)}")
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

                ulong roleId = 1;
                var node = PermissionNode.EventCreate;

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    await repository.DenyRolePermissionAsync(calendar.Id, roleId, node);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Single(context.Permissions);

                    var dbPermission = await context.Permissions
                        .Include(p => p.Calendar)
                        .FirstAsync();

                    Assert.Equal(calendar.Id, dbPermission.Calendar.Id);
                    Assert.Equal(roleId, dbPermission.TargetId);
                    Assert.Equal(PermissionType.Everyone, dbPermission.Type);
                    Assert.Equal(node, dbPermission.Node);
                }
            }

            [Fact]
            public async Task DoesNothingWhenPermissionAlreadyDenied()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(DoesNothingWhenPermissionAlreadyDenied)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                ulong roleId = 2;
                var node = PermissionNode.EventCreate;

                var permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    Calendar = calendar,
                    TargetId = roleId,
                    Type = PermissionType.Role,
                    Node = node
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.Permissions.AddAsync(permission);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    await repository.DenyRolePermissionAsync(calendar.Id, roleId, node);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Single(context.Permissions);

                    var dbPermission = await context.Permissions
                        .Include(p => p.Calendar)
                        .FirstAsync();

                    Assert.Equal(calendar.Id, dbPermission.Calendar.Id);
                    Assert.Equal(roleId, dbPermission.TargetId);
                    Assert.Equal(node, dbPermission.Node);
                }
            }
        }

        public class DenyUserPermissionAsyncMethod
        {
            [Fact]
            public async Task DeniesPermissionForUser()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(DeniesPermissionForUser)}")
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

                ulong userId = 2;
                var node = PermissionNode.EventCreate;

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    await repository.DenyUserPermissionAsync(calendar.Id, userId, node);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Single(context.Permissions);

                    var dbPermission = await context.Permissions
                        .Include(p => p.Calendar)
                        .FirstAsync();

                    Assert.Equal(calendar.Id, dbPermission.Calendar.Id);
                    Assert.Equal(userId, dbPermission.TargetId);
                    Assert.Equal(PermissionType.User, dbPermission.Type);
                    Assert.Equal(node, dbPermission.Node);
                }
            }

            [Fact]
            public async Task DoesNothingWhenPermissionAlreadyDenied()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(DoesNothingWhenPermissionAlreadyDenied)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                ulong userId = 2;
                var node = PermissionNode.EventCreate;

                var permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    Calendar = calendar,
                    TargetId = userId,
                    Type = PermissionType.User,
                    Node = node
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Calendars.AddAsync(calendar);
                    await context.Permissions.AddAsync(permission);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    await repository.DenyUserPermissionAsync(calendar.Id, userId, node);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    Assert.Single(context.Permissions);

                    var dbPermission = await context.Permissions
                        .Include(p => p.Calendar)
                        .FirstAsync();

                    Assert.Equal(calendar.Id, dbPermission.Calendar.Id);
                    Assert.Equal(userId, dbPermission.TargetId);
                    Assert.Equal(node, dbPermission.Node);
                }
            }
        }

        public class GetByIdAsyncMethod
        {
            [Fact]
            public async Task ReturnsPermission()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsPermission)}")
                    .Options;

                var permission = new Permission
                {
                    Id = Guid.NewGuid()
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Permissions.AddAsync(permission);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.GetByIdAsync(permission.Id);

                    Assert.Equal(permission.Id, result.Id);
                }
            }
        }

        public class GetForNodeAsyncMethod
        {
            [Fact]
            public async Task ReturnsPermissionsForNode()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsPermissionsForNode)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                var node = PermissionNode.EventCreate;
                var permissions = new List<Permission>
                {
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Permissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.GetForNodeAsync(calendar.Id, node);

                    Assert.Equal(permissions.Count, result.Count);
                    foreach (var permission in permissions)
                    {
                        Assert.Contains(result, r => r.Id == permission.Id);
                    }
                }
            }
        }

        public class GetForRoleAsyncMethod
        {
            [Fact]
            public async Task ReturnsPermissionsForRole()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsPermissionsForRole)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                ulong roleId = 2;
                var permissions = new List<Permission>
                {
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        TargetId = roleId,
                        Type = PermissionType.Role
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        TargetId = roleId,
                        Type = PermissionType.Role
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Permissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.GetForRoleAsync(calendar.Id, roleId);

                    Assert.Equal(permissions.Count, result.Count);
                    foreach (var permission in permissions)
                    {
                        Assert.Contains(result, r => r.Id == permission.Id);
                    }
                }
            }

            [Fact]
            public async Task ReturnsPermissionsForEveryoneRole()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsPermissionsForEveryoneRole)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                ulong roleId = 1;
                var permissions = new List<Permission>
                {
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        TargetId = roleId,
                        Type = PermissionType.Everyone
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        TargetId = roleId,
                        Type = PermissionType.Everyone
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Permissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.GetForRoleAsync(calendar.Id, roleId);

                    Assert.Equal(permissions.Count, result.Count);
                    foreach (var permission in permissions)
                    {
                        Assert.Contains(result, r => r.Id == permission.Id);
                    }
                }
            }
        }

        public class GetForUserAsyncMethod
        {
            [Fact]
            public async Task ReturnsPermissionsForUser()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsPermissionsForUser)}")
                    .Options;

                var calendar = new Calendar
                {
                    Id = 1
                };

                ulong userId = 1;
                var permissions = new List<Permission>
                {
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        TargetId = userId,
                        Type = PermissionType.User
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        TargetId = userId,
                        Type = PermissionType.User
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Permissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.GetForUserAsync(calendar.Id, userId);

                    Assert.Equal(permissions.Count, result.Count);
                    foreach (var permission in permissions)
                    {
                        Assert.Contains(result, r => r.Id == permission.Id);
                    }
                }
            }
        }

        public class ListAllAsyncMethod
        {
            [Fact]
            public async Task ReturnsPermissions()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsPermissions)}")
                    .Options;

                var permissions = new List<Permission>
                {
                    new Permission { Id = Guid.NewGuid() },
                    new Permission { Id = Guid.NewGuid() }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Permissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.ListAllAsync();

                    Assert.Equal(permissions.Count, result.Count);
                    foreach (var permission in permissions)
                    {
                        Assert.Contains(result, r => r.Id == permission.Id);
                    }
                }
            }
        }

        public class ListAsyncMethod
        {
            [Fact]
            public async Task ReturnsPermissions()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(ReturnsPermissions)}")
                    .Options;

                var permissions = new List<Permission>
                {
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Node = PermissionNode.EventCreate
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Node = PermissionNode.EventCreate
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid(),
                        Node = PermissionNode.EventDelete
                    }
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Permissions.AddRangeAsync(permissions);
                    await context.SaveChangesAsync();
                }

                var mockSpecification = SetupBasePermissionSpecification(new Mock<ISpecification<Permission>>());
                mockSpecification.Setup(x => x.Criteria)
                    .Returns(c => c.Node == PermissionNode.EventCreate);

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    var result = await repository.ListAsync(mockSpecification.Object);

                    Assert.Equal(2, result.Count);
                    for (int i = 0; i <= 1; i++)
                    {
                        Assert.Contains(result, x => x.Id == permissions[i].Id);
                    }
                }
            }
        }

        public class UpdateAsyncMethod
        {
            [Fact]
            public async Task UpdatesPermissions()
            {
                var options = new DbContextOptionsBuilder<SchedulerBotDbContext>()
                    .UseInMemoryDatabase(databaseName: $"{GetType().DeclaringType.Name}_{GetType().Name}_{nameof(UpdatesPermissions)}")
                    .Options;

                var permission = new Permission
                {
                    Id = Guid.NewGuid(),
                    TargetId = 1
                };

                using (var context = new SchedulerBotDbContext(options))
                {
                    await context.Permissions.AddAsync(permission);
                    await context.SaveChangesAsync();
                }

                ulong newTargetId = 2;
                permission.TargetId = newTargetId;

                using (var context = new SchedulerBotDbContext(options))
                {
                    var repository = new PermissionRepository(context);

                    await repository.UpdateAsync(permission);
                }

                using (var context = new SchedulerBotDbContext(options))
                {
                    var dbPermission = await context.Permissions.FirstAsync();

                    Assert.Equal(newTargetId, dbPermission.TargetId);
                }
            }
        }

        public static Mock<ISpecification<Permission>> SetupBasePermissionSpecification(Mock<ISpecification<Permission>> mock)
        {
            mock.Setup(x => x.Criteria)
                .Returns(_ => true);

            mock.Setup(x => x.Includes)
                .Returns(new List<Expression<Func<Permission, object>>>());

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
