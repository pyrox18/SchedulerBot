using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SchedulerBot.Client.Commands
{
    [Group("help")]
    public class HelpCommands : BaseCommandModule
    {
        [GroupCommand]
        public async Task Help(CommandContext ctx)
        {
            await ctx.RespondAsync("Help");
        }

        [Command("init")]
        public async Task Init(CommandContext ctx)
        {
            await ctx.RespondAsync("Help Init");
        }

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync("Help Ping");
        }

        [Command("prefix")]
        public async Task Prefix(CommandContext ctx)
        {
            await ctx.RespondAsync("Help Prefix");
        }

        [Command("info")]
        public async Task Info(CommandContext ctx)
        {
            await ctx.RespondAsync("Help Info");
        }

        [Command("support")]
        public async Task Support(CommandContext ctx)
        {
            await ctx.RespondAsync("Help Support");
        }
        
        [Command("time")]
        public async Task Time(CommandContext ctx)
        {
            await ctx.RespondAsync("Help Time");
        }

        [Group("event")]
        public class EventHelpCommands
        {
            [GroupCommand]
            public async Task Help(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Event");
            }

            [Command("create")]
            public async Task Create(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Create");
            }

            [Command("list")]
            public async Task List(CommandContext ctx)
            {
                await ctx.RespondAsync("Help List");
            }

            [Command("update")]
            public async Task Update(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Update");
            }

            [Command("delete")]
            public async Task Delete(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Delete");
            }

            [Command("rsvp")]
            public async Task Rsvp(CommandContext ctx)
            {
                await ctx.RespondAsync("Help RSVP");
            }
        }

        [Group("perms")]
        public class PermissionHelpCommands
        {
            [GroupCommand]
            public async Task Help(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Perms");
            }

            [Command("deny")]
            public async Task Deny(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Deny");
            }

            [Command("allow")]
            public async Task Allow(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Allow");
            }

            [Command("show")]
            public async Task Show(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Show");
            }

            [Command("nodes")]
            public async Task Nodes(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Nodes");
            }
        }

        [Group("settings")]
        public class SettingsHelpCommands
        {
            [GroupCommand]
            public async Task Help(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Settings");
            }

            [Command("timezone")]
            public async Task Timezone(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Timezone");
            }

            [Command("defaultchannel")]
            public async Task DefaultChannel(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Default Channel");
            }

            [Command("prefix")]
            public async Task Prefix(CommandContext ctx)
            {
                await ctx.RespondAsync("Help Prefix");
            }
        }
    }
}
