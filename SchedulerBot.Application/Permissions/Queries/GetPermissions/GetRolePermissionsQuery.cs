using MediatR;
using SchedulerBot.Application.Permissions.Models;

namespace SchedulerBot.Application.Permissions.Queries.GetPermissions
{
    public class GetRolePermissionsQuery : IRequest<RolePermissionsViewModel>
    {
        public ulong CalendarId { get; set; }
        public ulong RoleId { get; set; }
    }
}
