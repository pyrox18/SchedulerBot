using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace SchedulerBot.Client.Commands
{
    [Group("event", CanInvokeWithoutSubcommand = true)]
    [Description("Commands for managing events. Invoke without a subcommand to create an event.")]
    public class EventCommands
    {
        public async Task ExecuteGroupAsync(CommandContext ctx, string args)
        {
            await ctx.RespondAsync(args);
        }

        [Command("list"), Description("Lists all events.")]
        public async Task List(CommandContext ctx)
        {
            await ctx.RespondAsync("Event list");
        }

        [Command("update"), Description("Update an event.")]
        public async Task Update(CommandContext ctx, uint index, [RemainingText] string args)
        {
            await ctx.RespondAsync($"Updating event {index} with args {args}");
        }

        [Command("delete"), Description("Delete an event.")]
        public async Task Delete(CommandContext ctx, uint index)
        {
            await ctx.RespondAsync($"Deleting event {index}");
        }
    }
}
