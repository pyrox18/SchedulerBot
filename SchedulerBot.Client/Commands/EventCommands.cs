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
using SchedulerBot.Data.Exceptions;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;
using System.Globalization;

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
            if (string.IsNullOrEmpty(timezone))
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }

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

            Event savedEvent;
            try
            {
                savedEvent = await _eventService.CreateEventAsync(ctx.Guild.Id, evt);
            }
            catch (CalendarNotFoundException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
            var embed = EventEmbedFactory.GetCreateEventEmbed(savedEvent);

            await ctx.RespondAsync("New event created.", embed: embed);
        }

        [Command("list"), Description("Lists all events.")]
        public async Task List(CommandContext ctx)
        {
            List<Event> events;
            try
            {
                events = await _eventService.GetEventsAsync(ctx.Guild.Id);
            }
            catch (CalendarNotFoundException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("```css");

            if (events.Count < 1)
            {
                sb.AppendLine("No events found!");
            }
            else
            {
                int i = 0;
                bool activeEventHeaderWritten = false;
                DateTimeOffset now = DateTimeOffset.Now;

                while (i < events.Count && events[i].StartTimestamp <= now)
                {
                    if (!activeEventHeaderWritten)
                    {
                        sb.AppendLine("[Active Events]");
                        sb.AppendLine();
                        activeEventHeaderWritten = true;
                    }
                    sb.AppendLine($"{i + 1}: {events[i].Name} /* {events[i].StartTimestamp.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture)} to {events[i].EndTimestamp.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture)} */");
                    if (!string.IsNullOrEmpty(events[i].Description))
                    {
                        sb.AppendLine($"    # {events[i].Description}");
                    }
                    if (events[i].Repeat != RepeatType.None)
                    {
                        sb.AppendLine($"    # Repeat: {events[i].Repeat}");
                    }

                    i++;
                }
                if (i < events.Count)
                {
                    sb.AppendLine();
                    sb.AppendLine("[Upcoming Events]");
                    sb.AppendLine();
                }
                while (i < events.Count)
                {
                    sb.AppendLine($"{i + 1}: {events[i].Name} /* {events[i].StartTimestamp.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture)} to {events[i].EndTimestamp.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture)} */");
                    if (!string.IsNullOrEmpty(events[i].Description))
                    {
                        sb.AppendLine($"    # {events[i].Description}");
                    }
                    if (events[i].Repeat != RepeatType.None)
                    {
                        sb.AppendLine($"    # Repeat: {events[i].Repeat}");
                    }

                    i++;
                }
            }
            sb.AppendLine("```");

            await ctx.RespondAsync(sb.ToString());
        }

        [Command("update"), Description("Update an event.")]
        public async Task Update(CommandContext ctx, int index, [RemainingText] string args)
        {
            if (index <= 0)
            {
                await ctx.RespondAsync("Event index must be greater than 0.");
                return;
            }

            var timezone = await _calendarService.GetCalendarTimezoneAsync(ctx.Guild.Id);
            if (string.IsNullOrEmpty(timezone))
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }

            Event evt;
            try
            {
                evt = await _eventService.GetEventByIndexAsync(ctx.Guild.Id, index - 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                await ctx.RespondAsync("Event not found.");
                return;
            }

            Event updatedEvent;
            try
            {
                updatedEvent = EventParser.ParseUpdateEvent(evt, args, timezone);
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

            Event savedEvent = await _eventService.UpdateEventAsync(evt);
            var embed = EventEmbedFactory.GetUpdateEventEmbed(savedEvent);

            await ctx.RespondAsync("Updated event.", embed: embed);
        }

        [Command("delete"), Description("Delete an event.")]
        public async Task Delete(CommandContext ctx, int index)
        {
            if (index <= 0)
            {
                await ctx.RespondAsync("Event index must be greater than 0.");
                return;
            }
            Event deletedEvent;
            try
            {
                deletedEvent = await _eventService.DeleteEventAsync(ctx.Guild.Id, index - 1);
            }
            catch (ArgumentOutOfRangeException)
            {
                await ctx.RespondAsync("Event not found.");
                return;
            }
            catch (CalendarNotFoundException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }

            if (deletedEvent == null)
            {
                await ctx.RespondAsync("Event not found.");
                return;
            }

            var embed = EventEmbedFactory.GetDeleteEventEmbed(deletedEvent);
            await ctx.RespondAsync("Deleted event.", embed: embed);
        }
    }
}
