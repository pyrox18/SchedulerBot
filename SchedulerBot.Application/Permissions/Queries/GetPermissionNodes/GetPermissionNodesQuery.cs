using MediatR;
using SchedulerBot.Application.Permissions.Models;

namespace SchedulerBot.Application.Permissions.Queries.GetPermissionNodes
{
    public class GetPermissionNodesQuery : IRequest<PermissionNodeViewModel>
    {
    }
}
