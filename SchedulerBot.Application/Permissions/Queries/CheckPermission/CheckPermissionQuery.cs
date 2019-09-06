using MediatR;
using SchedulerBot.Application.Permissions.Enumerations;
using SchedulerBot.Application.Permissions.Models;
using System.Collections.Generic;

namespace SchedulerBot.Application.Permissions.Queries.CheckPermission
{
    public class CheckPermissionQuery : IRequest<PermissionCheckViewModel>
    {
        public ulong CalendarId { get; set; }
        public PermissionNode Node { get; set; }
        public ulong UserId { get; set; }
        public List<ulong> RoleIds { get; set; }
    }
}
