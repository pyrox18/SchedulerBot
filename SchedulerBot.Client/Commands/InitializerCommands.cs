using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Configuration;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Commands
{
    public class InitializerCommands : BaseCommandModule
    {
        private readonly ICalendarService _calendarService;
        private readonly IConfigurationRoot _configuration;

        public InitializerCommands(ICalendarService calendarService, IConfigurationRoot configuration)
        {
            _calendarService = calendarService;
            _configuration = configuration;
        }

        [Command("init"), Description("Initialize the bot with a timezone and a default channel.")]
        public async Task Initialize(CommandContext ctx, string timezone)
        {
            await ctx.TriggerTypingAsync();

            bool? initSuccess;
            initSuccess = await _calendarService.InitialiseCalendar(ctx.Guild.Id, timezone, ctx.Channel.Id);

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
