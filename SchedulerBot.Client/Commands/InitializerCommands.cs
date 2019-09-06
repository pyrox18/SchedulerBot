using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MediatR;
using Microsoft.Extensions.Configuration;
using SchedulerBot.Application.Calendars.Commands.InitialiseCalendar;
using SchedulerBot.Application.Exceptions;

namespace SchedulerBot.Client.Commands
{
    public class InitializerCommands : BotCommandModule
    {
        private readonly IConfigurationRoot _configuration;

        public InitializerCommands(IMediator mediator, IConfigurationRoot configuration) :
            base(mediator)
        {
            _configuration = configuration;
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
                Prefix = _configuration.GetSection("Bot").GetSection("Prefixes").Get<string[]>()[0]
            };

            var validator = new InitialiseCalendarCommandValidator();
            var validationResult = validator.Validate(command);
            if (!validationResult.IsValid)
            {
                var timezoneLink = _configuration.GetSection("Bot").GetSection("Links").GetValue<string>("TimezoneList");
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
