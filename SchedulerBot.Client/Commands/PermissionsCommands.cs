using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Data.Exceptions;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Commands
{
    [Group("perms")] 
    [Description("View and modify permissions for other commands.")]
    public class PermissionsCommands : BaseCommandModule
    {
        private readonly IPermissionService _permissionService;

        public PermissionsCommands(IPermissionService permissionService) => _permissionService = permissionService;

        [Command("allow"), Description("Allows a certain role to use a certain command.")]
        public async Task Allow(CommandContext ctx, string node, DiscordRole role)
        {
            Permission permission;
            try
            {
                permission = await _permissionService.AllowNodeForRoleAsync(ctx.Guild.Id, role.Id, node);
            }
            catch (PermissionNodeNotFoundException)
            {
                await ctx.RespondAsync("Permission node not found.");
                return;
            }
            await ctx.RespondAsync($"Allowed permission {permission.Node} for role {role.Name}.");
        }

        [Command("allow"), Description("Allows a certain user to use a certain command.")]
        public async Task Allow(CommandContext ctx, string node, DiscordMember user)
        {
            Permission permission;
            try
            {
                permission = await _permissionService.AllowNodeForUserAsync(ctx.Guild.Id, user.Id, node);
            }
            catch (PermissionNodeNotFoundException)
            {
                await ctx.RespondAsync("Permission node not found.");
                return;
            }
            await ctx.RespondAsync($"Allowed permission {permission.Node} for user {user.GetUsernameAndDiscriminator()}.");
        }

        [Command("deny"), Description("Denies a certain role from using a certain command.")]
        public async Task Deny(CommandContext ctx, string node, DiscordRole role)
        {
            Permission permission;
            try
            {
                permission = await _permissionService.DenyNodeForRoleAsync(ctx.Guild.Id, role.Id, node);
            }
            catch (PermissionNodeNotFoundException)
            {
                await ctx.RespondAsync("Permission node not found.");
                return;
            }
            await ctx.RespondAsync($"Denied permission {permission.Node} for role {role.Name}.");
        }

        [Command("deny"), Description("Denies a certain user from using a certain command.")]
        public async Task Deny(CommandContext ctx, string node, DiscordMember user)
        {
            Permission permission;
            try
            {
                permission = await _permissionService.DenyNodeForUserAsync(ctx.Guild.Id, user.Id, node);
            }
            catch (PermissionNodeNotFoundException)
            {
                await ctx.RespondAsync("Permission node not found.");
                return;
            }
            await ctx.RespondAsync($"Denied permission {permission.Node} for user {user.GetUsernameAndDiscriminator()}.");
        }

        [Command("show"), Description("Shows current permission settings for a role.")]
        public async Task Show(CommandContext ctx, DiscordRole role)
        {
            var permissions = await _permissionService.GetPermissionsForRoleAsync(ctx.Guild.Id, role.Id);

            var sb = new StringBuilder();
            sb.AppendLine("```css");
            sb.AppendLine($"Role: {role.Name}");
            sb.AppendLine("Denied Nodes:");
            if (permissions.Count < 1)
            {
                sb.AppendLine("  None");
            }
            else
            {
                foreach (var perm in permissions)
                {
                    sb.AppendLine($"  {perm.Node}");
                }
            }
            sb.AppendLine("```");

            await ctx.RespondAsync(sb.ToString());
        }

        [Command("show"), Description("Shows current permission settings for a user.")]
        public async Task Show(CommandContext ctx, DiscordMember user)
        {
            var permissions = await _permissionService.GetPermissionsForUserAsync(ctx.Guild.Id, user.Id);

            var sb = new StringBuilder();
            sb.AppendLine("```css");
            sb.AppendLine($"User: {user.GetUsernameAndDiscriminator()}");
            sb.AppendLine("Denied Nodes:");
            if (permissions.Count < 1)
            {
                sb.AppendLine("  None");
            }
            else
            {
                foreach (var perm in permissions)
                {
                    sb.AppendLine($"  {perm.Node}");
                }
            }
            sb.AppendLine("```");

            await ctx.RespondAsync(sb.ToString());
        }

        [Command("show"), Description("Shows current permission settings for a node.")]
        public async Task Show(CommandContext ctx, string node)
        {
            List<Permission> permissions;
            try
            {
                permissions = await _permissionService.GetPermissionsForNodeAsync(ctx.Guild.Id, node);
            }
            catch (PermissionNodeNotFoundException)
            {
                await ctx.RespondAsync("Permission node not found.");
                return;
            }

            var roleNames = new List<string>();
            var userNames = new List<string>();

            foreach (var perm in permissions)
            {
                if (perm.Type == PermissionType.Role)
                {
                    DiscordRole role = ctx.Guild.Roles.FirstOrDefault(r => r.Id == perm.TargetId);
                    if (role != null)
                    {
                        roleNames.Add(role.Name);
                    }
                }
                else
                {
                    DiscordMember user = ctx.Guild.Members.FirstOrDefault(m => m.Id == perm.TargetId);
                    if (user != null)
                    {
                        userNames.Add(user.GetUsernameAndDiscriminator());
                    }
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("```css");
            sb.AppendLine($"Node: {node}");
            sb.AppendLine("Denied Roles:");
            if (roleNames.Count < 1)
            {
                sb.AppendLine("  None");
            }
            else
            {
                foreach (var name in roleNames)
                {
                    sb.AppendLine($"  {name}");
                }
            }
            sb.AppendLine("Denied Users:");
            if (userNames.Count < 1)
            {
                sb.AppendLine("  None");
            }
            else
            {
                foreach (var name in userNames)
                {
                    sb.AppendLine($"  {name}");
                }
            }
            sb.AppendLine("```");

            await ctx.RespondAsync(sb.ToString());
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
