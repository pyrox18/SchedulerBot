﻿using SchedulerBot.Domain.Enumerations;
using System;

namespace SchedulerBot.Domain.Models
{
    public class Permission
    {
        public Guid Id { get; set; }
        public Calendar Calendar { get; set; }
        public PermissionType Type { get; set; }
        public PermissionNode Node { get; set; }
        public ulong TargetId { get; set; }
        public bool IsDenied { get; set; }
    }
}
