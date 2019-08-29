using SchedulerBot.Application.Permissions.Models;
using SchedulerBot.Application.Permissions.Queries.GetPermissionNodes;
using System.Threading.Tasks;
using Xunit;

namespace SchedulerBot.Application.UnitTests.Permissions.Queries.GetPermissionNodes
{
    public class GetPermissionNodesQueryHandlerFacts
    {
        public class HandleGetPermissionNodesQuery
        {
            [Fact]
            public async Task ReturnsViewModel()
            {
                var query = new GetPermissionNodesQuery();

                var handler = new GetPermissionNodesQueryHandler();
                var result = await handler.Handle(query);

                Assert.IsType<PermissionNodeViewModel>(result);
                Assert.NotEmpty(result.PermissionNodes);
            }
        }
    }
}
