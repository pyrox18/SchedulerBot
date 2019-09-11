using MediatR;

namespace SchedulerBot.Application.Permissions.Commands.DeleteUserPermissions
{
    public class DeleteUserPermissionsCommand : IRequest
    {
        public ulong CalendarId { get; set; }
        public ulong UserId { get; set; }
    }
}
