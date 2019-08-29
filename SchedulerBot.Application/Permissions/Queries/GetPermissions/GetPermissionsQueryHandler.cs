using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Permissions.Models;

namespace SchedulerBot.Application.Permissions.Queries.GetPermissions
{
    public class GetPermissionsQueryHandler :
        IRequestHandler<GetUserPermissionsQuery, UserPermissionsViewModel>
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
    }
}
