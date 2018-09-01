using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Configuration;
using RedLockNet;
using SchedulerBot.Client.Exceptions;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Commands
{
    public class InitializerCommands : BaseCommandModule
    {
        private readonly ICalendarService _calendarService;
        private readonly IConfigurationRoot _configuration;
        private readonly IDistributedLockFactory _redlockFactory;

        public InitializerCommands(ICalendarService calendarService, IConfigurationRoot configuration, IDistributedLockFactory redlockFactory)
        {
            _calendarService = calendarService;
            _configuration = configuration;
            _redlockFactory = redlockFactory;
        }

        [Command("init"), Description("Initialize the bot with a timezone and a default channel.")]
        public async Task Initialize(CommandContext ctx, string timezone)
        {
            await ctx.TriggerTypingAsync();

            bool? initSuccess;
            using (var redlock = await _redlockFactory.CreateLockAsync(ctx.Guild.Id.ToString(), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(0.5)))
            {
                if (redlock.IsAcquired)
                {
                    initSuccess = await _calendarService.InitialiseCalendar(ctx.Guild.Id, timezone, ctx.Channel.Id);
                }
                else
                {
                    throw new RedisLockAcquireException($"Cannot acquire lock for guild {ctx.Guild.Id}");
                }
            }

            if (initSuccess == null)
            {
                await ctx.RespondAsync("Timezone already initialised.");
            }
            else if (initSuccess == false)
            {
                var timezoneLink = _configuration.GetSection("Bot").GetSection("Links").GetValue<string>("TimezoneList");
                await ctx.RespondAsync($"Timezone not found. See {timezoneLink} under the TZ column for a list of valid timezones.");
            }
            else
            {
                await ctx.RespondAsync($"Set calendar timezone to {timezone} and default channel to {ctx.Channel.Mention}.");
            }
        }
    }
}
