using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Attributes
{
    public class PermissionNodeAttribute : CheckBaseAttribute
    {
        private readonly PermissionNode _node;

        public PermissionNodeAttribute(PermissionNode node)
        {
            Console.WriteLine($"PermissionNodeAttribute with node {node.ToString()} initialising");
            _node = node;
        }

        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            Console.WriteLine($"PermissionNodeAttribute with node {_node.ToString()} executing check");
            var permissionService = ctx.Services.GetRequiredService<IPermissionService>();

            if (ctx.Member.IsOwner)
            {
                return true;
            }

            var permitted = await permissionService.CheckPermissionsAsync(
                _node, ctx.Guild.Id, ctx.Member.Id,
                ctx.Member.Roles.Select(r => r.Id));

            if (!permitted)
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
            }

            return permitted;
        }
    }
}
