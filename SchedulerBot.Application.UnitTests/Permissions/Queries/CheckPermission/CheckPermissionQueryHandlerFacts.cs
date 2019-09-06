using Moq;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Permissions.Queries.CheckPermission;
using SchedulerBot.Data.Enumerations;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Permissions.Queries.CheckPermission
{
    public class CheckPermissionQueryHandlerFacts
    {
        public class HandleCheckPermissionQueryMethod
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var mockPermissionRepository = new Mock<IPermissionRepository>();
                mockPermissionRepository.Setup(x => x.CheckPermissionAsync(It.IsAny<ulong>(), It.IsAny<PermissionNode>(), It.IsAny<ulong>(), It.IsAny<List<ulong>>()))
                    .ReturnsAsync(true);

                var query = new CheckPermissionQuery
                {
                    CalendarId = 1,
                    Node = Application.Permissions.Enumerations.PermissionNode.EventCreate,
                    UserId = 2,
                    RoleIds = new List<ulong> { 3, 4 }
                };

                var handler = new CheckPermissionQueryHandler(mockPermissionRepository.Object);
                var result = await handler.Handle(query);

                Assert.True(result.IsPermitted);
            }
        }
    }
}
