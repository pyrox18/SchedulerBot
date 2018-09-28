using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using SchedulerBot.Client.Attributes;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Scheduler;
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
        private readonly IEventService _eventService;
        private readonly IPermissionService _permissionService;
        private readonly IEventScheduler _eventScheduler;
        private readonly IConfigurationRoot _configuration;

        public SettingsCommands(ICalendarService calendarService, IEventService eventService, IPermissionService permissionService, IEventScheduler eventScheduler, IConfigurationRoot configuration)
        {
            _calendarService = calendarService;
            _eventService = eventService;
            _permissionService = permissionService;
            _eventScheduler = eventScheduler;
            _configuration = configuration;
        }

        [GroupCommand]
        public async Task Settings(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

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
        [PermissionNode(PermissionNode.PrefixShow)]
        public async Task ShowPrefix(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            if (!await this.CheckPermission(_permissionService, typeof(SettingsCommands), nameof(SettingsCommands.ShowPrefix), ctx.Member))
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
                return;
            }

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
        [PermissionNode(PermissionNode.PrefixModify)]
        public async Task ModifyPrefix(CommandContext ctx, string prefix)
        {
            await ctx.TriggerTypingAsync();

            if (!await this.CheckPermission(_permissionService, typeof(SettingsCommands), nameof(SettingsCommands.ModifyPrefix), ctx.Member))
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
                return;
            }

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
        [PermissionNode(PermissionNode.DefaultChannelShow)]
        public async Task ShowDefaultChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            if (!await this.CheckPermission(_permissionService, typeof(SettingsCommands), nameof(SettingsCommands.ShowDefaultChannel), ctx.Member))
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
                return;
            }

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
        [PermissionNode(PermissionNode.DefaultChannelModify)]
        public async Task ModifyDefaultChannel(CommandContext ctx, DiscordChannel channel)
        {
            await ctx.TriggerTypingAsync();

            if (!await this.CheckPermission(_permissionService, typeof(SettingsCommands), nameof(SettingsCommands.ModifyDefaultChannel), ctx.Member))
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
                return;
            }

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

            await RescheduleAllEvents(ctx, defaultChannel);

            await ctx.RespondAsync($"Updated default channel to {defaultChannel.AsChannelMention()}.");
        }

        [Command("timezone"), Description("View the timezone for the bot.")]
        [PermissionNode(PermissionNode.TimezoneShow)]
        public async Task ShowTimezone(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            if (!await this.CheckPermission(_permissionService, typeof(SettingsCommands), nameof(SettingsCommands.ShowTimezone), ctx.Member))
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
                return;
            }

            var timezone = await _calendarService.GetCalendarTimezoneAsync(ctx.Guild.Id);
            if (string.IsNullOrEmpty(timezone))
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }

            var timezoneLink = _configuration.GetSection("Bot").GetSection("Links").GetValue<string>("TimezoneList");
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = "SchedulerBot",
                    IconUrl = "https://cdn.discordapp.com/avatars/339019867325726722/e5fca7dbae7156e05c013766fa498fe1.png"
                },
                Color = new DiscordColor(211, 255, 219),
                Description = $"Run `settings timezone <new timezone>` to change the timezone. e.g. `settings timezone America/Los_Angeles`\nSee {timezoneLink} under the TZ column for a list of valid timezones.",
                Title = "Settings: Timezone"
            };
            embed.AddField("Current Value", $"{timezone}", true);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("timezone"), Description("Change the timezone for the bot.")]
        [PermissionNode(PermissionNode.TimezoneModify)]
        public async Task ModifyTimezone(CommandContext ctx, string timezone)
        {
            await ctx.TriggerTypingAsync();

            if (!await this.CheckPermission(_permissionService, typeof(SettingsCommands), nameof(SettingsCommands.ModifyTimezone), ctx.Member))
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
                return;
            }

            string tz = string.Empty;
            try
            {
                tz = await _calendarService.UpdateCalendarTimezoneAsync(ctx.Guild.Id, timezone);
            }
            catch (InvalidTimeZoneException)
            {
                var timezoneLink = _configuration.GetSection("Bot").GetSection("Links").GetValue<string>("TimezoneList");
                await ctx.RespondAsync($"Timezone not found. See {timezoneLink} under the TZ column for a list of valid timezones.");
                return;
            }
            catch (ExistingEventInNewTimezonePastException)
            {
                await ctx.RespondAsync($"Cannot update timezone, due to events starting or ending in the past if the timezone is changed to {timezone}.");
                return;
            }
            catch (CalendarNotFoundException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }

            var defaultChannel = await _calendarService.GetCalendarDefaultChannelAsync(ctx.Guild.Id);
            await RescheduleAllEvents(ctx, defaultChannel);

            await ctx.RespondAsync($"Updated timezone to {tz}.");
        }

        private async Task RescheduleAllEvents(CommandContext ctx, ulong channelId)
        {
            var events = await _eventService.GetEventsAsync(ctx.Guild.Id);
            foreach (var evt in events)
            {
                await _eventScheduler.RescheduleEvent(evt, ctx.Client, channelId);
            }
        }
    }
}
