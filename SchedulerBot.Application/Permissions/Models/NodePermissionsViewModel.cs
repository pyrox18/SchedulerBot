using SchedulerBot.Domain.Enumerations;
using SchedulerBot.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace SchedulerBot.Application.Permissions.Models
{
    public class NodePermissionsViewModel
    {
        public bool IsEveryoneDenied { get; set; }
        public List<ulong> DeniedUserIds { get; set; }
        public List<ulong> DeniedRoleIds { get; set; }

        public static NodePermissionsViewModel FromPermissions(IEnumerable<Permission> permissions)
        {
            var vm = new NodePermissionsViewModel();

            if (permissions.Any(p => p.Type == PermissionType.Everyone))
            {
                vm.IsEveryoneDenied = true;
            }

            vm.DeniedUserIds = permissions.Where(p => p.Type == PermissionType.User)
                .Select(p => p.TargetId)
                .ToList();
            vm.DeniedRoleIds = permissions.Where(p => p.Type == PermissionType.Role)
                .Select(p => p.TargetId)
                .ToList();

            return vm;
        }
    }
}
