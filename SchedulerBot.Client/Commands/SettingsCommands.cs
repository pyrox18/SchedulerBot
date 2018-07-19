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
    [Group("settings")]
    [Description("Change settings for the bot.")]
    public class SettingsCommands : BaseCommandModule
    {
        [GroupCommand]
        public async Task Settings(CommandContext ctx)
        {
            await ctx.RespondAsync("Settings");
        }

        [Command("prefix"), Description("View the bot's prefix.")]
        public async Task Prefix(CommandContext ctx)
        {
            await ctx.RespondAsync("`-`");
        }

        [Command("prefix"), Description("Change the bot's prefix.")]
        public async Task Prefix(CommandContext ctx, string prefix)
        {
            await ctx.RespondAsync($"Changing prefix to {prefix}");
        }

        [Command("defaultchannel"), Description("View the default channel that the bot sends messages to.")]
        public async Task DefaultChannel(CommandContext ctx)
        {
            await ctx.RespondAsync("Default channel");
        }

        [Command("defaultchannel"), Description("Set the default channel that the bot sends messages to.")]
        public async Task DefaultChannel(CommandContext ctx, DiscordChannel channel)
        {
            await ctx.RespondAsync($"Changing default channel to {channel.Mention}");
        }

        [Command("timezone"), Description("View the timezone for the bot.")]
        public async Task Timezone(CommandContext ctx)
        {
            await ctx.RespondAsync("Timezone");
        }

        [Command("timezone"), Description("Change the timezone for the bot.")]
        public async Task Timezone(CommandContext ctx, string timezone)
        {
            await ctx.RespondAsync($"Changing timezone to {timezone}");
        }
    }
}
