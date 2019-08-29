using SchedulerBot.Application.Permissions.Enumerations;
using System;
using System.Collections.Generic;

namespace SchedulerBot.Application.Permissions.Models
{
    public class PermissionNodeViewModel
    {
        public List<string> PermissionNodes { get; set; }

        public PermissionNodeViewModel()
        {
            PermissionNodes = new List<string>(Enum.GetNames(typeof(PermissionNode)));
        }
    }
}
