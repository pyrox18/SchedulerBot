using MediatR;
using SchedulerBot.Application.Permissions.Enumerations;

namespace SchedulerBot.Application.Permissions.Commands.ModifyUserPermission
{
    public class AllowUserPermissionCommand : IRequest
    {
        public ulong CalendarId { get; set; }
        public ulong UserId { get; set; }
        public PermissionNode Node { get; set; }
    }
}
