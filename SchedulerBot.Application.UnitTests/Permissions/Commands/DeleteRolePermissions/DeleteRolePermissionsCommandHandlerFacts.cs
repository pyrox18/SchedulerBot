using MediatR;
using Moq;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Permissions.Commands.DeleteRolePermissions;
using SchedulerBot.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Permissions.Commands.DeleteRolePermissions
{
    public class DeleteRolePermissionsCommandHandlerFacts
    {
        public class HandleDeleteRolePermissionsCommandMethod
        {
            [Fact]
            public async Task ReturnsUnit()
            {
                var permissions = new List<Permission>
                {
                    new Permission
                    {
                        Id = Guid.NewGuid()
                    },
                    new Permission
                    {
                        Id = Guid.NewGuid()
                    }
                };

                var mockPermissionRepository = new Mock<IPermissionRepository>();
                mockPermissionRepository.Setup(x => x.GetForRoleAsync(It.IsAny<ulong>(), It.IsAny<ulong>()))
                    .ReturnsAsync(permissions)
                    .Verifiable();
                mockPermissionRepository.Setup(x => x.DeleteAsync(It.IsAny<Permission>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

                var command = new DeleteRolePermissionsCommand
                {
                    CalendarId = 1,
                    RoleId = 2
                };

                var handler = new DeleteRolePermissionsCommandHandler(mockPermissionRepository.Object);
                var result = await handler.Handle(command);

                mockPermissionRepository.Verify();
                Assert.Equal(Unit.Value, result);
            }
        }
    }
}
