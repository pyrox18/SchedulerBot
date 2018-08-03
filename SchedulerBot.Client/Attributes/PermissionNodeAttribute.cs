using System;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Client.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PermissionNodeAttribute : Attribute
    {
        public PermissionNode Node { get; set; }

        public PermissionNodeAttribute(PermissionNode node)
        {
            Node = node;
        }
    }
}
