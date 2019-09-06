using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SchedulerBot.Application.Permissions.Enumerations;
using SchedulerBot.Application.Permissions.Queries.CheckPermission;

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

            if (ctx.Member.IsOwner)
            {
                return true;
            }

            var mediator = ctx.Services.GetRequiredService<IMediator>();

            var result = await mediator.Send(new CheckPermissionQuery
            {
                CalendarId = ctx.Guild.Id,
                Node = _node,
                UserId = ctx.Member.Id,
                RoleIds = ctx.Member.Roles.Select(r => r.Id).ToList()
            });

            if (!result.IsPermitted)
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
            }

            return result.IsPermitted;
        }
    }
}
