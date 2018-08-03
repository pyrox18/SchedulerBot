using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SchedulerBot.Client.Commands
{
    [Hidden, RequireOwner]
    public class AdminCommands : BaseCommandModule
    {
        [Command("admincheck"), Description("Checks if the user is the bot owner.")]
        public async Task AdminCheck(CommandContext ctx)
        {
            await ctx.RespondAsync("Yes");
        }

        [Command("forceerror"), Description("Forces the bot to throw an error.")]
        public async Task ForceError(CommandContext ctx)
        {
            await ctx.RespondAsync("Forcing error");
            throw new Exception("Test exception");
        }

        [Command("shell"), Description("Run command-line instructions.")]
        public async Task Shell(CommandContext ctx, string command)
        {
            await ctx.RespondAsync($"Executing command: {command}");
        }
    }
}
