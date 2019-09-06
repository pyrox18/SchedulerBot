using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Enumerations;

namespace SchedulerBot.Application.Permissions.Commands.ModifyRolePermission
{
    public class ModifyRolePermissionCommandHandler :
        IRequestHandler<DenyRolePermissionCommand>,
        IRequestHandler<AllowRolePermissionCommand>
    {
        private readonly IPermissionRepository _permissionRepository;

        public ModifyRolePermissionCommandHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<Unit> Handle(DenyRolePermissionCommand request, CancellationToken cancellationToken = default)
        {
            var node = (PermissionNode)request.Node;

            await _permissionRepository.DenyRolePermissionAsync(request.CalendarId, request.RoleId, node);

            return Unit.Value;
        }

        public async Task<Unit> Handle(AllowRolePermissionCommand request, CancellationToken cancellationToken = default)
        {
            var node = (PermissionNode)request.Node;

            await _permissionRepository.AllowRolePermissionAsync(request.CalendarId, request.RoleId, node);

            return Unit.Value;
        }
    }
}
