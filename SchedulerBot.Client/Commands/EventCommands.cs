using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SchedulerBot.Client.Attributes;
using SchedulerBot.Client.EmbedFactories;
using SchedulerBot.Client.Exceptions;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Parsers;
using SchedulerBot.Data.Exceptions;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;
using SchedulerBot.Client.Scheduler;

namespace SchedulerBot.Client.Commands
{
    [Group("event")]
    [Description("Commands for managing events.")]
    public class EventCommands : BaseCommandModule
    {
        private readonly ICalendarService _calendarService;
        private readonly IEventService _eventService;
        private readonly IPermissionService _permissionService;
        private readonly IEventScheduler _eventScheduler;

        public EventCommands(ICalendarService calendarService, IEventService eventService, IPermissionService permissionService, IEventScheduler eventScheduler)
        {
            _calendarService = calendarService;
            _eventService = eventService;
            _permissionService = permissionService;
            _eventScheduler = eventScheduler;
        }

        [GroupCommand, Description("Create an event.")]
        [PermissionNode(PermissionNode.EventCreate)]
        public async Task Create(CommandContext ctx, params string[] args)
        {
            if (!await this.CheckPermission(_permissionService, typeof(EventCommands), nameof(EventCommands.Create), ctx.Member))
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

            var defaultChannelId = await _calendarService.GetCalendarDefaultChannelAsync(ctx.Guild.Id);
            await _eventScheduler.ScheduleEvent(savedEvent, ctx.Client, defaultChannelId);

            var embed = EventEmbedFactory.GetCreateEventEmbed(savedEvent);
            await ctx.RespondAsync("New event created.", embed: embed);
        }

        [Command("list"), Description("Lists all events.")]
        [PermissionNode(PermissionNode.EventList)]
        public async Task List(CommandContext ctx)
        {
            if (!await this.CheckPermission(_permissionService, typeof(EventCommands), nameof(EventCommands.List), ctx.Member))
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
                return;
            }

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
        [PermissionNode(PermissionNode.EventUpdate)]
        public async Task Update(CommandContext ctx, int index, [RemainingText] string args)
        {
            if (!await this.CheckPermission(_permissionService, typeof(EventCommands), nameof(EventCommands.Update), ctx.Member))
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
                return;
            }

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
            var defaultChannelId = await _calendarService.GetCalendarDefaultChannelAsync(ctx.Guild.Id);
            await _eventScheduler.RescheduleEvent(evt, ctx.Client, defaultChannelId);

            var embed = EventEmbedFactory.GetUpdateEventEmbed(savedEvent);
            await ctx.RespondAsync("Updated event.", embed: embed);
        }

        [Command("delete"), Description("Delete an event.")]
        [PermissionNode(PermissionNode.EventDelete)]
        public async Task Delete(CommandContext ctx, int index)
        {
            if (!await this.CheckPermission(_permissionService, typeof(EventCommands), nameof(EventCommands.Delete), ctx.Member))
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
                return;
            }

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

            await _eventScheduler.UnscheduleEvent(deletedEvent);

            var embed = EventEmbedFactory.GetDeleteEventEmbed(deletedEvent);
            await ctx.RespondAsync("Deleted event.", embed: embed);
        }
    }
}
