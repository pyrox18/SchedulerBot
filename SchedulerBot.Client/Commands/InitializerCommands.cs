using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Commands
{
    public class InitializerCommands : BaseCommandModule
    {
        private readonly ICalendarService _calendarService;

        public InitializerCommands(ICalendarService calendarService) => _calendarService = calendarService;

        [Command("init"), Description("Initialize the bot with a timezone and a default channel.")]
        public async Task Initialize(CommandContext ctx, string timezone)
        {
            await ctx.TriggerTypingAsync();

            var initSuccess = await _calendarService.InitialiseCalendar(ctx.Guild.Id, timezone, ctx.Channel.Id);
            if (initSuccess == null)
            {
                await ctx.RespondAsync("Timezone already initialised.");
            }
            else if (initSuccess == false)
            {
                await ctx.RespondAsync($"Timezone not found. See https://goo.gl/NzNMon under the TZ column for a list of valid timezones.");
            }
            else
            {
                await ctx.RespondAsync($"Set calendar timezone to {timezone} and default channel to {ctx.Channel.Mention}.");
            }
        }
    }
}
