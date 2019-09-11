using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Interfaces;

namespace SchedulerBot.Application.Permissions.Commands.DeleteUserPermissions
{
    public class DeleteUserPermissionsCommandHandler : IRequestHandler<DeleteUserPermissionsCommand>
    {
        private readonly IPermissionRepository _permissionRepository;

        public DeleteUserPermissionsCommandHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<Unit> Handle(DeleteUserPermissionsCommand request, CancellationToken cancellationToken = default)
        {
            var permissions = await _permissionRepository.GetForUserAsync(request.CalendarId, request.UserId);

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
