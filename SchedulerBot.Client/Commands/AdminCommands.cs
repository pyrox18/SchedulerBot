using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SchedulerBot.Client.Commands
{
    [Hidden, RequireOwner]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class AdminCommands : BaseCommandModule
    {
        [Command("admincheck"), Description("Checks if the user is the bot owner.")]
        public async Task AdminCheck(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("Yes");
        }

        [Command("forceerror"), Description("Forces the bot to throw an error.")]
        public async Task ForceError(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("Forcing error");
            throw new Exception("Test exception");
        }
    }
}
