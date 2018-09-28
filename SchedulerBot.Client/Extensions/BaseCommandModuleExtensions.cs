using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using SchedulerBot.Client.Attributes;
using SchedulerBot.Client.Exceptions;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Extensions
{
    public static class BaseCommandModuleExtensions
    {
        public static async Task<bool> CheckPermission(this BaseCommandModule module, IPermissionService permissionService, Type t, string methodName, DiscordMember member)
        {
            if (member.IsOwner)
            {
                return true;
            }

            var method = t.GetMethod(methodName);
            PermissionNodeAttribute attribute = method.GetCustomAttribute<PermissionNodeAttribute>();
            if (attribute == null)
            {
                throw new PermissionNodeAttributeNullException();
            }

            var node = attribute.Node;
            return await permissionService.CheckPermissionsAsync(node, member.Guild.Id, member.Id, member.Roles.Select(r => r.Id).ToList());
        }
    }
}
