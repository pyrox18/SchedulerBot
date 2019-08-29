using MediatR;
using SchedulerBot.Application.Permissions.Models;

namespace SchedulerBot.Application.Permissions.Queries.GetPermissions
{
    public class GetUserPermissionsQuery : IRequest<UserPermissionsViewModel>
    {
        public ulong CalendarId { get; set; }
        public ulong UserId { get; set; }
    }
}
