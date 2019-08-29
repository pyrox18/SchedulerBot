using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Permissions.Models;

namespace SchedulerBot.Application.Permissions.Queries.GetPermissionNodes
{
    public class GetPermissionNodesQueryHandler : IRequestHandler<GetPermissionNodesQuery, PermissionNodeViewModel>
    {
        public Task<PermissionNodeViewModel> Handle(GetPermissionNodesQuery request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new PermissionNodeViewModel());
        }
    }
}
