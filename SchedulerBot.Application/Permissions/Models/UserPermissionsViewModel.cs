using SchedulerBot.Application.Permissions.Enumerations;
using SchedulerBot.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace SchedulerBot.Application.Permissions.Models
{
    public class UserPermissionsViewModel
    {
        public List<Enumerations.PermissionNode> DeniedNodes { get; set; }

        public static UserPermissionsViewModel FromPermissions(IEnumerable<Permission> permissions)
        {
            var nodes = permissions.Select(p => p.Node)
                .Select(n => (Enumerations.PermissionNode)(int)n)
                .ToList();

            return new UserPermissionsViewModel
            {
                DeniedNodes = nodes
            };
        }
    }
}
