using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using NodaTime;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Commands
{
    [Group("settings")]
    [Description("Change settings for the bot.")]
    public class SettingsCommands : BaseCommandModule
    {
        private readonly ICalendarService _calendarService;

        public SettingsCommands(ICalendarService calendarService) => _calendarService = calendarService;

        [GroupCommand]
        public async Task Settings(CommandContext ctx)
        {
            var calendar = await _calendarService.TryGetCalendarAsync(ctx.Guild.Id);
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = "SchedulerBot",
                    IconUrl = "https://cdn.discordapp.com/avatars/339019867325726722/e5fca7dbae7156e05c013766fa498fe1.png"
                },
                Color = new DiscordColor(211, 255, 219),
                Description = "Run `settings <setting>` to view more details. e.g. `settings prefix`",
                Title = "Settings"
            };
            embed.AddField("prefix", $"Current value: `{calendar.Prefix}`", true);
            embed.AddField("defaultchannel", $"Current value: {calendar.DefaultChannel.AsChannelMention()}", true);
            embed.AddField("timezone", $"Current value: {calendar.Timezone}", true);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("prefix"), Description("View the bot's prefix.")]
        public async Task Prefix(CommandContext ctx)
        {
            var prefix = await _calendarService.GetCalendarPrefixAsync(ctx.Guild.Id);
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = "SchedulerBot",
                    IconUrl = "https://cdn.discordapp.com/avatars/339019867325726722/e5fca7dbae7156e05c013766fa498fe1.png"
                },
                Color = new DiscordColor(211, 255, 219),
                Description = "Run `settings prefix <new prefix>` to change the prefix. e.g. `settings prefix ++`",
                Title = "Settings: Prefix"
            };
            embed.AddField("Current Value", $"`{prefix}`", true);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("prefix"), Description("Change the bot's prefix.")]
        public async Task Prefix(CommandContext ctx, string prefix)
        {
            string newPrefix = await _calendarService.UpdateCalendarPrefixAsync(ctx.Guild.Id, prefix);
            await ctx.RespondAsync($"Prefix set to `{newPrefix}`.");
        }

        [Command("defaultchannel"), Description("View the default channel that the bot sends messages to.")]
        public async Task DefaultChannel(CommandContext ctx)
        {
            var defaultChannel = await _calendarService.GetCalendarDefaultChannelAsync(ctx.Guild.Id);
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = "SchedulerBot",
                    IconUrl = "https://cdn.discordapp.com/avatars/339019867325726722/e5fca7dbae7156e05c013766fa498fe1.png"
                },
                Color = new DiscordColor(211, 255, 219),
                Description = "Run `settings defaultchannel #newchannel` to change the default channel. e.g. `settings defaultchannel #general`",
                Title = "Settings: Default Channel"
            };
            embed.AddField("Current Value", $"{defaultChannel.AsChannelMention()}", true);

            await ctx.RespondAsync(embed: embed);
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
            var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timezone);
            if (tz == null)
            {
                await ctx.RespondAsync($"Timezone {timezone} does not exist.");
            }
            else
            {
                await ctx.RespondAsync($"Changing timezone to {timezone}");
            }
        }
    }
}
