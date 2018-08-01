using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Commands
{
    [Group("perms")] 
    [Description("View and modify permissions for other commands.")]
    public class PermissionsCommands : BaseCommandModule
    {
        private readonly IPermissionService _permissionService;

        public PermissionsCommands(IPermissionService permissionService) => _permissionService = permissionService;

        [Command("allow"), Description("Allows a certain role or user to use a certain command.")]
        public async Task Allow(CommandContext ctx, string args)
        {
            await ctx.RespondAsync($"Allowing permissions: {args}");
        }

        [Command("deny"), Description("Denies a certain role or user from using a certain command.")]
        public async Task Deny(CommandContext ctx, string args)
        {
            await ctx.RespondAsync($"Denying permissions: {args}");
        }

        [Command("show"), Description("Shows current permission settings for a node, role or user.")]
        public async Task Show(CommandContext ctx, string args)
        {
            await ctx.RespondAsync($"Showing permissions: {args}");
        }

        [Command("nodes"), Description("Lists all available permission nodes.")]
        public async Task Nodes(CommandContext ctx)
        {
            var nodes = _permissionService.GetPermissionNodes();
            var sb = new StringBuilder();
            sb.AppendLine("```css");
            foreach (var node in nodes)
            {
                sb.AppendLine(node);
            }
            sb.AppendLine("```");

            await ctx.RespondAsync(sb.ToString());
        }
    }
}
