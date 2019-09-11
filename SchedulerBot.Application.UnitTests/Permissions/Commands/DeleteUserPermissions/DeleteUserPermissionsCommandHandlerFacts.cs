using MediatR;
using Moq;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Permissions.Commands.DeleteUserPermissions;
using SchedulerBot.Data.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Permissions.Commands.DeleteUserPermissions
{
    public class DeleteUserPermissionsCommandHandlerFacts
    {
        public class HandleDeleteUserPermissionsCommandMethod
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
                mockPermissionRepository.Setup(x => x.GetForUserAsync(It.IsAny<ulong>(), It.IsAny<ulong>()))
                    .ReturnsAsync(permissions)
                    .Verifiable();
                mockPermissionRepository.Setup(x => x.DeleteAsync(It.IsAny<Permission>()))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

                var command = new DeleteUserPermissionsCommand
                {
                    CalendarId = 1,
                    UserId = 2
                };

                var handler = new DeleteUserPermissionsCommandHandler(mockPermissionRepository.Object);
                var result = await handler.Handle(command);

                mockPermissionRepository.Verify();
                Assert.Equal(Unit.Value, result);
            }
        }
    }
}
