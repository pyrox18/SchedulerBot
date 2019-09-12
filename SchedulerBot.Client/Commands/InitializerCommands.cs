using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SchedulerBot.Application.Calendars.Commands.InitialiseCalendar;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Client.Configuration;

namespace SchedulerBot.Client.Commands
{
    public class InitializerCommands : BotCommandModule
    {
        private readonly BotConfiguration _configuration;

        public InitializerCommands(IMediator mediator, IOptions<BotConfiguration> configuration) :
            base(mediator)
        {
            _configuration = configuration.Value;
        }

        [Command("init"), Description("Initialize the bot with a timezone and a default channel.")]
        public async Task Initialize(CommandContext ctx, string timezone)
        {
            await ctx.TriggerTypingAsync();

            var command = new InitialiseCalendarCommand
            {
                CalendarId = ctx.Guild.Id,
                Timezone = timezone,
                ChannelId = ctx.Channel.Id,
                Prefix = _configuration.Prefixes[0]
            };

            var validator = new InitialiseCalendarCommandValidator();
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
                await ctx.RespondAsync($"Set calendar timezone to {result.Timezone} and default channel to {ctx.Channel.Mention}.");
            }
            catch (CalendarAlreadyInitialisedException)
            {
                await ctx.RespondAsync("Timezone already initialised.");
            }
        }
    }
}
