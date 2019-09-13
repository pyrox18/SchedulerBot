using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Permissions.Models;
using SchedulerBot.Domain.Enumerations;

namespace SchedulerBot.Application.Permissions.Queries.CheckPermission
{
    public class CheckPermissionQueryHandler : IRequestHandler<CheckPermissionQuery, PermissionCheckViewModel>
    {
        private readonly IPermissionRepository _permissionRepository;

        public CheckPermissionQueryHandler(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public async Task<PermissionCheckViewModel> Handle(CheckPermissionQuery request, CancellationToken cancellationToken = default)
        {
            var node = (PermissionNode)request.Node;

            var isPermitted = await _permissionRepository.CheckPermissionAsync(request.CalendarId, node, request.UserId, request.RoleIds);

            return new PermissionCheckViewModel(isPermitted);
        }
    }
}
