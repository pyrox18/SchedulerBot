﻿using MediatR;
using SchedulerBot.Application.Permissions.Enumerations;

namespace SchedulerBot.Application.Permissions.Commands.ModifyRolePermission
{
    public class AllowRolePermissionCommand : IRequest
    {
        public ulong CalendarId { get; set; }
        public ulong RoleId { get; set; }
        public PermissionNode Node { get; set; }
    }
}
