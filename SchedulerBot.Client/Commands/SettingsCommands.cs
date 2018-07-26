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
using SchedulerBot.Data.Exceptions;
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
            if (calendar == null)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
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
            if (string.IsNullOrEmpty(prefix))
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
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
            string newPrefix = string.Empty;
            try
            {
                newPrefix = await _calendarService.UpdateCalendarPrefixAsync(ctx.Guild.Id, prefix);
            }
            catch (CalendarNotFoundException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }

            await ctx.RespondAsync($"Prefix set to `{newPrefix}`.");
        }

        [Command("defaultchannel"), Description("View the default channel that the bot sends messages to.")]
        public async Task DefaultChannel(CommandContext ctx)
        {
            var defaultChannel = await _calendarService.GetCalendarDefaultChannelAsync(ctx.Guild.Id);
            if (defaultChannel == 0)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }

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
            ulong defaultChannel = 0;
            try
            {
                defaultChannel = await _calendarService.UpdateCalendarDefaultChannelAsync(ctx.Guild.Id, channel.Id);
            }
            catch (CalendarNotFoundException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
            await ctx.RespondAsync($"Updated default channel to {defaultChannel.AsChannelMention()}.");
        }

        [Command("timezone"), Description("View the timezone for the bot.")]
        public async Task Timezone(CommandContext ctx)
        {
            var timezone = await _calendarService.GetCalendarTimezoneAsync(ctx.Guild.Id);
            if (string.IsNullOrEmpty(timezone))
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = "SchedulerBot",
                    IconUrl = "https://cdn.discordapp.com/avatars/339019867325726722/e5fca7dbae7156e05c013766fa498fe1.png"
                },
                Color = new DiscordColor(211, 255, 219),
                Description = "Run `settings timezone <new timezone>` to change the timezone. e.g. `settings timezone America/Los_Angeles`\nSee https://goo.gl/NzNMon under the TZ column for a list of valid timezones.",
                Title = "Settings: Timezone"
            };
            embed.AddField("Current Value", $"{timezone}", true);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("timezone"), Description("Change the timezone for the bot.")]
        public async Task Timezone(CommandContext ctx, string timezone)
        {
            string tz = string.Empty;

            try
            {
                tz = await _calendarService.UpdateCalendarTimezoneAsync(ctx.Guild.Id, timezone);
            }
            catch (InvalidTimeZoneException)
            {
                await ctx.RespondAsync($"Timezone not found. See https://goo.gl/NzNMon under the TZ column for a list of valid timezones.");
                return;
            }
            catch (CalendarNotFoundException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }

            await ctx.RespondAsync($"Updated timezone to {tz}.");
        }
    }
}
