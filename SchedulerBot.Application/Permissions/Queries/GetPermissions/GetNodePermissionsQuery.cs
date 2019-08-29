using MediatR;
using SchedulerBot.Application.Permissions.Enumerations;
using SchedulerBot.Application.Permissions.Models;

namespace SchedulerBot.Application.Permissions.Queries.GetPermissions
{
    public class GetNodePermissionsQuery : IRequest<NodePermissionsViewModel>
    {
        public ulong CalendarId { get; set; }
        public PermissionNode Node { get; set; }
    }
}
