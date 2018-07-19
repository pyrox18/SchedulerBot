using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace SchedulerBot.Client.Commands
{
    [Group("settings", CanInvokeWithoutSubcommand = true)]
    [Description("Change settings for the bot.")]
    public class SettingsCommands
    {
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("Settings");
        }

        [Command("prefix"), Description("View or change the bot's prefix.")]
        public async Task Prefix(CommandContext ctx, string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                await ctx.RespondAsync("`-`");
            }
            else
            {
                await ctx.RespondAsync($"Changing prefix to {prefix}");
            }
        }

        [Command("defaultchannel"), Description("View or set the default channel that the bot sends messages to.")]
        public async Task DefaultChannel(CommandContext ctx, DiscordChannel channel = null)
        {
            if (channel == null)
            {
                await ctx.RespondAsync("Default channel");
            }
            else
            {
                await ctx.RespondAsync($"Changing default channel to {channel.Mention}");
            }
        }

        [Command("timezone"), Description("Change the timezone for the bot.")]
        public async Task Timezone(CommandContext ctx, string timezone)
        {
            if (string.IsNullOrEmpty(timezone))
            {
                await ctx.RespondAsync("Timezone");
            }
            else
            {
                await ctx.RespondAsync($"Changing timezone to {timezone}");
            }
        }
    }
}
