using MediatR;

namespace SchedulerBot.Application.Permissions.Commands.DeleteRolePermissions
{
    public class DeleteRolePermissionsCommand : IRequest
    {
        public ulong CalendarId { get; set; }
        public ulong RoleId { get; set; }
    }
}
