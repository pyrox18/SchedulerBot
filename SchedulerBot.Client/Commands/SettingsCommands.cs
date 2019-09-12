using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SchedulerBot.Application.Events.Queries.GetEvents;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Permissions.Enumerations;
using SchedulerBot.Application.Settings.Commands.ModifySetting;
using SchedulerBot.Application.Settings.Queries.GetAllSettings;
using SchedulerBot.Application.Settings.Queries.GetSetting;
using SchedulerBot.Client.Attributes;
using SchedulerBot.Client.Configuration;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Scheduler;

namespace SchedulerBot.Client.Commands
{
    [Group("settings")]
    [Description("Change settings for the bot.")]
    public class SettingsCommands : BotCommandModule
    {
        private readonly IEventScheduler _eventScheduler;
        private readonly BotConfiguration _configuration;
        private readonly IMemoryCache _cache;

        public SettingsCommands(
            IMediator mediator,
            IEventScheduler eventScheduler,
            IOptions<BotConfiguration> configuration,
            IMemoryCache cache) :
            base(mediator)
        {
            _eventScheduler = eventScheduler;
            _configuration = configuration.Value;
            _cache = cache;
        }

        [GroupCommand]
        public async Task Settings(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                var result = await _mediator.Send(new GetAllSettingsQuery
                {
                    CalendarId = ctx.Guild.Id
                });

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
                embed.AddField("prefix", $"Current value: `{result.Prefix}`", true);
                embed.AddField("defaultchannel", $"Current value: {result.DefaultChannel.AsChannelMention()}", true);
                embed.AddField("timezone", $"Current value: {result.Timezone}", true);

                await ctx.RespondAsync(embed: embed);
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
        }

        [Command("prefix"), Description("View the bot's prefix.")]
        [PermissionNode(PermissionNode.PrefixShow)]
        public async Task ShowPrefix(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                var result = await _mediator.Send(new GetPrefixSettingQuery
                {
                    CalendarId = ctx.Guild.Id
                });

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
                embed.AddField("Current Value", $"`{result.Prefix}`", true);

                await ctx.RespondAsync(embed: embed);
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
        }

        [Command("prefix"), Description("Change the bot's prefix.")]
        [PermissionNode(PermissionNode.PrefixModify)]
        public async Task ModifyPrefix(CommandContext ctx, string prefix)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                var result = await _mediator.Send(new ModifyPrefixSettingCommand
                {
                    CalendarId = ctx.Guild.Id,
                    NewPrefix = prefix
                });

                // Update prefix cache
                _cache.Set($"prefix:{ctx.Guild.Id}", result.Prefix, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(3)));

                await ctx.RespondAsync($"Prefix set to `{result.Prefix}`.");
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
        }

        [Command("defaultchannel"), Description("View the default channel that the bot sends messages to.")]
        [PermissionNode(PermissionNode.DefaultChannelShow)]
        public async Task ShowDefaultChannel(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                var result = await _mediator.Send(new GetDefaultChannelSettingQuery
                {
                    CalendarId = ctx.Guild.Id
                });

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
                embed.AddField("Current Value", $"{result.DefaultChannel.AsChannelMention()}", true);

                await ctx.RespondAsync(embed: embed);
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
        }

        [Command("defaultchannel"), Description("Set the default channel that the bot sends messages to.")]
        [PermissionNode(PermissionNode.DefaultChannelModify)]
        public async Task ModifyDefaultChannel(CommandContext ctx, DiscordChannel channel)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                var result = await _mediator.Send(new ModifyDefaultChannelSettingCommand
                {
                    CalendarId = ctx.Guild.Id,
                    NewDefaultChannel = channel.Id
                });

                await RescheduleAllEvents(ctx, result.DefaultChannel);

                await ctx.RespondAsync($"Updated default channel to {result.DefaultChannel.AsChannelMention()}.");
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
        }

        [Command("timezone"), Description("View the timezone for the bot.")]
        [PermissionNode(PermissionNode.TimezoneShow)]
        public async Task ShowTimezone(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                var result = await _mediator.Send(new GetTimezoneSettingQuery
                {
                    CalendarId = ctx.Guild.Id
                });

                var timezoneLink = _configuration.Links.TimezoneList;
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
                embed.AddField("Current Value", $"{result.Timezone}", true);

                await ctx.RespondAsync(embed: embed);
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
        }

        [Command("timezone"), Description("Change the timezone for the bot.")]
        [PermissionNode(PermissionNode.TimezoneModify)]
        public async Task ModifyTimezone(CommandContext ctx, string timezone)
        {
            await ctx.TriggerTypingAsync();

            var command = new ModifyTimezoneSettingCommand
            {
                CalendarId = ctx.Guild.Id,
                NewTimezone = timezone
            };

            var validator = new ModifyTimezoneSettingCommandValidator();
            var validationResult = validator.Validate(command);
            if (!validationResult.IsValid)
            {
                var timezoneLink = _configuration.Links.TimezoneList;
                await ctx.RespondAsync($"Timezone not found. See {timezoneLink} under the TZ column for a list of valid timezones.");
                return;
            }

            try
            {
                var result = await _mediator.Send(command);

                await RescheduleAllEvents(ctx, result.DefaultChannel);

                await ctx.RespondAsync($"Updated timezone to {result.Timezone}.");
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
            catch (EventStartInNewTimezonePastException)
            {
                await ctx.RespondAsync($"Cannot update timezone, due to events starting or ending in the past if the timezone is changed to {timezone}.");
                return;
            }
        }

        private async Task RescheduleAllEvents(CommandContext ctx, ulong channelId)
        {
            var events = await _mediator.Send(new GetEventsForCalendarQuery
            {
                CalendarId = ctx.Guild.Id
            });

            foreach (var evt in events)
            {
                await _eventScheduler.RescheduleEvent(evt, ctx.Client.ShardId, channelId);
            }
        }
    }
}
