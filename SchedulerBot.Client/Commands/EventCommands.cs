using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SchedulerBot.Client.EmbedFactories;
using SchedulerBot.Client.Exceptions;
using SchedulerBot.Client.Parsers;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Commands
{
    [Group("event")]
    [Description("Commands for managing events.")]
    public class EventCommands : BaseCommandModule
    {
        private readonly ICalendarService _calendarService;
        private readonly IEventService _eventService;

        public EventCommands(ICalendarService calendarService, IEventService eventService)
        {
            _calendarService = calendarService;
            _eventService = eventService;
        }

        [GroupCommand, Description("Create an event.")]
        public async Task Create(CommandContext ctx, params string[] args)
        {
            var timezone = await _calendarService.GetCalendarTimezoneAsync(ctx.Guild.Id);
            Event evt;
            try
            {
                evt = EventParser.ParseNewEvent(args, timezone);
            }
            catch (DateTimeInPastException)
            {
                await ctx.RespondAsync("Cannot create an event that starts or ends in the past.");
                return;
            }
            catch (EventEndBeforeStartException)
            {
                await ctx.RespondAsync("Cannot create an event that ends before it starts.");
                return;
            }
            catch (EventParseException)
            {
                await ctx.RespondAsync("Failed to parse event data.");
                return;
            }

            var savedEvent = await _eventService.CreateEventAsync(ctx.Guild.Id, evt);
            var embed = EventEmbedFactory.GetCreateEventEmbed(savedEvent);

            await ctx.RespondAsync("New event created.", embed: embed);
        }

        [Command("list"), Description("Lists all events.")]
        public async Task List(CommandContext ctx)
        {
            await ctx.RespondAsync("Event list");
        }

        [Command("update"), Description("Update an event.")]
        public async Task Update(CommandContext ctx, uint index, [RemainingText] string args)
        {
            await ctx.RespondAsync($"Updating event {index} with args {args}");
        }

        [Command("delete"), Description("Delete an event.")]
        public async Task Delete(CommandContext ctx, uint index)
        {
            await ctx.RespondAsync($"Deleting event {index}");
        }
    }
}
