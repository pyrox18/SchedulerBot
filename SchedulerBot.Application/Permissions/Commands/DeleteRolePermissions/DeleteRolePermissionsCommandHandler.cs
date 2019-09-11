using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Interfaces;

namespace SchedulerBot.Application.Permissions.Commands.DeleteRolePermissions
{
    public class DeleteRolePermissionsCommandHandler : IRequestHandler<DeleteRolePermissionsCommand>
    {
        private readonly IPermissionRepository _permissionRepository;

        public DeleteRolePermissionsCommandHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<Unit> Handle(DeleteRolePermissionsCommand request, CancellationToken cancellationToken = default)
        {
            var permissions = await _permissionRepository.GetForRoleAsync(request.CalendarId, request.RoleId);

            var tasks = new List<Task>();
            foreach (var permission in permissions)
            {
                tasks.Add(_permissionRepository.DeleteAsync(permission));
            }

            await Task.WhenAll(tasks);

            return Unit.Value;
        }
    }
}
