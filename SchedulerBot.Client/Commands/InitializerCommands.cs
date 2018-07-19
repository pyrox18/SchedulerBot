using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SchedulerBot.Client.Commands
{
    public class InitializerCommands
    {
        [Command("init"), Description("Initialize the bot with a timezone and a default channel.")]
        public async Task Initialize(CommandContext ctx, string timezone)
        {
            await ctx.RespondAsync($"Initializing to timezone {timezone}");
        }
    }
}
