using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SchedulerBot.Client.Commands
{
    public class MiscCommands : BaseCommandModule
    {
        [Command("ping"), Description("Pings the bot.")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.RespondAsync("Pong!");
        }

        [Command("prefix"), Description("View the bot's current prefix for your guild.")]
        public async Task Prefix(CommandContext ctx)
        {
            await ctx.RespondAsync("`-`");
        }

        [Command("info"), Description("Get some information about the bot.")]
        public async Task Info(CommandContext ctx)
        {
            await ctx.RespondAsync("Info");
        }

        [Command("support"), Description("Get an invite link to the SchedulerBot support server.")]
        public async Task Support(CommandContext ctx)
        {
            await ctx.RespondAsync("Support link");
        }

        [Command("invite"), Description("Get a link to invite the bot to your server.")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.RespondAsync("Invite link");
        }
    }
}
