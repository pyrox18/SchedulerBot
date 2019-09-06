using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Permissions.Commands.ModifyRolePermission;
using SchedulerBot.Application.Permissions.Commands.ModifyUserPermission;
using SchedulerBot.Application.Permissions.Enumerations;
using SchedulerBot.Application.Permissions.Queries.GetPermissionNodes;
using SchedulerBot.Application.Permissions.Queries.GetPermissions;
using SchedulerBot.Client.Attributes;
using SchedulerBot.Client.Extensions;

namespace SchedulerBot.Client.Commands
{
    [Group("perms")] 
    [Description("View and modify permissions for other commands.")]
    public class PermissionsCommands : BotCommandModule
    {
        public PermissionsCommands(IMediator mediator) :
            base(mediator)
        {
        }

        [Command("allow"), Description("Allows a certain role to use a certain command.")]
        [PermissionNode(PermissionNode.PermsModify)]
        public async Task AllowRole(CommandContext ctx, string node, DiscordRole role)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                var permissionNode = GetPermissionNode(node);

                await _mediator.Send(new AllowRolePermissionCommand
                {
                    CalendarId = ctx.Guild.Id,
                    Node = permissionNode,
                    RoleId = role.Id
                });

                await ctx.RespondAsync($"Allowed permission {node} for role {role.Name}.");
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
            catch (Exceptions.PermissionNodeNotFoundException)
            {
                await ctx.RespondAsync("Permission node not found.");
                return;
            }
        }

        [Command("allow"), Description("Allows a certain user to use a certain command.")]
        [PermissionNode(PermissionNode.PermsModify)]
        public async Task AllowUser(CommandContext ctx, string node, DiscordMember user)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                var permissionNode = GetPermissionNode(node);

                await _mediator.Send(new AllowUserPermissionCommand
                {
                    CalendarId = ctx.Guild.Id,
                    Node = permissionNode,
                    UserId = user.Id
                });

                await ctx.RespondAsync($"Allowed permission {node} for user {user.GetUsernameAndDiscriminator()}.");
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
            catch (Exceptions.PermissionNodeNotFoundException)
            {
                await ctx.RespondAsync("Permission node not found.");
                return;
            }
        }

        [Command("deny"), Description("Denies a certain role from using a certain command.")]
        [PermissionNode(PermissionNode.PermsModify)]
        public async Task DenyRole(CommandContext ctx, string node, DiscordRole role)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                var permissionNode = GetPermissionNode(node);

                await _mediator.Send(new DenyRolePermissionCommand
                {
                    CalendarId = ctx.Guild.Id,
                    Node = permissionNode,
                    RoleId = role.Id
                });

                await ctx.RespondAsync($"Denied permission {node} for role {role.Name}.");
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
            catch (Exceptions.PermissionNodeNotFoundException)
            {
                await ctx.RespondAsync("Permission node not found.");
                return;
            }
        }

        [Command("deny"), Description("Denies a certain user from using a certain command.")]
        [PermissionNode(PermissionNode.PermsModify)]
        public async Task DenyUser(CommandContext ctx, string node, DiscordMember user)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                var permissionNode = GetPermissionNode(node);

                await _mediator.Send(new DenyUserPermissionCommand
                {
                    CalendarId = ctx.Guild.Id,
                    Node = permissionNode,
                    UserId = user.Id
                });

                await ctx.RespondAsync($"Denied permission {node} for role {user.GetUsernameAndDiscriminator()}.");
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
            catch (Exceptions.PermissionNodeNotFoundException)
            {
                await ctx.RespondAsync("Permission node not found.");
                return;
            }
        }

        [Command("show"), Description("Shows current permission settings for a role.")]
        [PermissionNode(PermissionNode.PermsShow)]
        public async Task ShowRole(CommandContext ctx, DiscordRole role)
        {
            await ctx.TriggerTypingAsync();

            var result = await _mediator.Send(new GetRolePermissionsQuery
            {
                CalendarId = ctx.Guild.Id,
                RoleId = role.Id
            });

            var sb = new StringBuilder();
            sb.AppendLine("```css");
            sb.AppendLine($"Role: {role.Name}");
            sb.AppendLine("Denied Nodes:");
            if (result.DeniedNodes.Count < 1)
            {
                sb.AppendLine("  None");
            }
            else
            {
                foreach (var node in result.DeniedNodes)
                {
                    sb.AppendLine($"  {node}");
                }
            }
            sb.AppendLine("```");

            await ctx.RespondAsync(sb.ToString());
        }

        [Command("show"), Description("Shows current permission settings for a user.")]
        [PermissionNode(PermissionNode.PermsShow)]
        public async Task ShowUser(CommandContext ctx, DiscordMember user)
        {
            await ctx.TriggerTypingAsync();

            var result = await _mediator.Send(new GetUserPermissionsQuery
            {
                CalendarId = ctx.Guild.Id,
                UserId = user.Id
            });

            var sb = new StringBuilder();
            sb.AppendLine("```css");
            sb.AppendLine($"User: {user.GetUsernameAndDiscriminator()}");
            sb.AppendLine("Denied Nodes:");
            if (result.DeniedNodes.Count < 1)
            {
                sb.AppendLine("  None");
            }
            else
            {
                foreach (var node in result.DeniedNodes)
                {
                    sb.AppendLine($"  {node}");
                }
            }
            sb.AppendLine("```");

            await ctx.RespondAsync(sb.ToString());
        }

        [Command("show"), Description("Shows current permission settings for a node.")]
        [PermissionNode(PermissionNode.PermsShow)]
        public async Task ShowNode(CommandContext ctx, string node)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                var permissionNode = GetPermissionNode(node);

                var result = await _mediator.Send(new GetNodePermissionsQuery
                {
                    CalendarId = ctx.Guild.Id,
                    Node = permissionNode
                });

                var roleNames = ctx.Guild.Roles
                    .Where(r => r.Key != ctx.Guild.Id && result.DeniedRoleIds.Contains(r.Key))
                    .Select(r => r.Value.Name)
                    .ToList();

                if (result.IsEveryoneDenied)
                {
                    roleNames.Insert(0, "@everyone");
                }

                var userNames = ctx.Guild.Members
                    .Where(m => result.DeniedUserIds.Contains(m.Key))
                    .Select(m => m.Value.GetUsernameAndDiscriminator())
                    .ToList();

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
            catch (Exceptions.PermissionNodeNotFoundException)
            {
                await ctx.RespondAsync("Permission node not found.");
                return;
            }
        }

        [Command("nodes"), Description("Lists all available permission nodes.")]
        [PermissionNode(PermissionNode.PermsNodes)]
        public async Task Nodes(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var result = await _mediator.Send(new GetPermissionNodesQuery());

            var sb = new StringBuilder();
            sb.AppendLine("```css");
            foreach (var node in result.PermissionNodes)
            {
                sb.AppendLine(node);
            }
            sb.AppendLine("```");

            await ctx.RespondAsync(sb.ToString());
        }

        private Application.Permissions.Enumerations.PermissionNode GetPermissionNode(string node)
        {
            var nodes = Enum.GetNames(typeof(Application.Permissions.Enumerations.PermissionNode));
            var actualNode = nodes.FirstOrDefault(n => n.ToLower() == node.ToLower());
            if (string.IsNullOrEmpty(actualNode))
            {
                throw new Exceptions.PermissionNodeNotFoundException(node);
            }

            return (Application.Permissions.Enumerations.PermissionNode)Enum.Parse(typeof(Application.Permissions.Enumerations.PermissionNode), actualNode);
        }
    }
}
