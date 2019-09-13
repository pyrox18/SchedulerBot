using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Domain.Enumerations;

namespace SchedulerBot.Application.Permissions.Commands.ModifyUserPermission
{
    public class ModifyUserPermissionCommandHandler :
        IRequestHandler<DenyUserPermissionCommand>,
        IRequestHandler<AllowUserPermissionCommand>
    {
        private readonly IPermissionRepository _permissionRepository;

        public ModifyUserPermissionCommandHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<Unit> Handle(DenyUserPermissionCommand request, CancellationToken cancellationToken = default)
        {
            var node = (PermissionNode)request.Node;

            await _permissionRepository.DenyUserPermissionAsync(request.CalendarId, request.UserId, node);

            return Unit.Value;
        }

        public async Task<Unit> Handle(AllowUserPermissionCommand request, CancellationToken cancellationToken = default)
        {
            var node = (PermissionNode)request.Node;

            await _permissionRepository.AllowUserPermissionAsync(request.CalendarId, request.UserId, node);

            return Unit.Value;
        }
    }
}
