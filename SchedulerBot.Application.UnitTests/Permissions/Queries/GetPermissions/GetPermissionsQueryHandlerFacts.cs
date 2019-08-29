using Moq;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Permissions.Queries.GetPermissions;
using SchedulerBot.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Permissions.Queries.GetPermissions
{
    public class GetPermissionsQueryHandlerFacts
    {
        public class HandleGetUserPermissionsQueryMethod
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var permissions = new List<Permission>
                {
                    new Permission
                    {
                        TargetId = 1,
                        Type = PermissionType.User,
                        Node = PermissionNode.EventCreate
                    },
                    new Permission
                    {
                        TargetId = 1,
                        Type = PermissionType.User,
                        Node = PermissionNode.EventDelete
                    }
                };

                var mockRepository = new Mock<IPermissionRepository>();
                mockRepository.Setup(x => x.GetForUserAsync(It.IsAny<ulong>(), It.IsAny<ulong>()))
                    .ReturnsAsync(permissions);

                var query = new GetUserPermissionsQuery
                {
                    CalendarId = 1,
                    UserId = 1
                };

                var handler = new GetPermissionsQueryHandler(mockRepository.Object);
                var result = await handler.Handle(query);

                Assert.Equal(permissions.Count, result.DeniedNodes.Count);
                Assert.Contains(Application.Permissions.Enumerations.PermissionNode.EventCreate, result.DeniedNodes);
                Assert.Contains(Application.Permissions.Enumerations.PermissionNode.EventDelete, result.DeniedNodes);
            }
        }

        public class HandleGetRolePermissionsQueryMethod
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var permissions = new List<Permission>
                {
                    new Permission
                    {
                        TargetId = 1,
                        Type = PermissionType.Role,
                        Node = PermissionNode.EventCreate
                    },
                    new Permission
                    {
                        TargetId = 1,
                        Type = PermissionType.Role,
                        Node = PermissionNode.EventDelete
                    }
                };

                var mockRepository = new Mock<IPermissionRepository>();
                mockRepository.Setup(x => x.GetForRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>()))
                    .ReturnsAsync(permissions);

                var query = new GetRolePermissionsQuery
                {
                    CalendarId = 1,
                    RoleId = 1
                };

                var handler = new GetPermissionsQueryHandler(mockRepository.Object);
                var result = await handler.Handle(query);

                Assert.Equal(permissions.Count, result.DeniedNodes.Count);
                Assert.Contains(Application.Permissions.Enumerations.PermissionNode.EventCreate, result.DeniedNodes);
                Assert.Contains(Application.Permissions.Enumerations.PermissionNode.EventDelete, result.DeniedNodes);
            }
        }
    }
}
