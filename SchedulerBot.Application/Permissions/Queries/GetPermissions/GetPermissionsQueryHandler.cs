using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Permissions.Models;
using SchedulerBot.Data.Enumerations;

namespace SchedulerBot.Application.Permissions.Queries.GetPermissions
{
    public class GetPermissionsQueryHandler :
        IRequestHandler<GetUserPermissionsQuery, UserPermissionsViewModel>,
        IRequestHandler<GetRolePermissionsQuery, RolePermissionsViewModel>,
        IRequestHandler<GetNodePermissionsQuery, NodePermissionsViewModel>
    {
        private readonly IPermissionRepository _permissionRepository;

        public GetPermissionsQueryHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }
        
        public async Task<UserPermissionsViewModel> Handle(GetUserPermissionsQuery request, CancellationToken cancellationToken = default)
        {
            var permissions = await _permissionRepository.GetForUserAsync(request.CalendarId, request.UserId);

            return UserPermissionsViewModel.FromPermissions(permissions);
        }

        public async Task<RolePermissionsViewModel> Handle(GetRolePermissionsQuery request, CancellationToken cancellationToken = default)
        {
            var permissions = await _permissionRepository.GetForRoleAsync(request.CalendarId, request.RoleId);

            return RolePermissionsViewModel.FromPermissions(permissions);
        }

        public async Task<NodePermissionsViewModel> Handle(GetNodePermissionsQuery request, CancellationToken cancellationToken = default)
        {
            var node = (PermissionNode)request.Node;
            var permissions = await _permissionRepository.GetForNodeAsync(request.CalendarId, node);

            return NodePermissionsViewModel.FromPermissions(permissions);
        }
    }
}
