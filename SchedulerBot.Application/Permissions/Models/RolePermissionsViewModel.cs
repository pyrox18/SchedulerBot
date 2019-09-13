using SchedulerBot.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace SchedulerBot.Application.Permissions.Models
{
    public class RolePermissionsViewModel
    {
        public List<Enumerations.PermissionNode> DeniedNodes { get; set; }

        public static RolePermissionsViewModel FromPermissions(IEnumerable<Permission> permissions)
        {
            var nodes = permissions.Select(p => p.Node)
                .Select(n => (Enumerations.PermissionNode)(int)n)
                .ToList();

            return new RolePermissionsViewModel
            {
                DeniedNodes = nodes
            };
        }
    }
}
