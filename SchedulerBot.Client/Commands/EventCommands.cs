using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using SchedulerBot.Client.Attributes;
using SchedulerBot.Client.Factories;
using SchedulerBot.Client.Exceptions;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Parsers;
using SchedulerBot.Client.Scheduler;
using SchedulerBot.Data.Exceptions;
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
            // Permission node check workaround for GroupCommand method
            var attr = (PermissionNodeAttribute)(
                GetType()
                .GetMethod("Create")
                .GetCustomAttributes(typeof(PermissionNodeAttribute), true)[0]);
            if (!(await attr.ExecuteCheckAsync(ctx, false)))
            {
                return;
            }

            await ctx.TriggerTypingAsync();

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
                await ctx.RespondAsync("Cannot create an event that starts or ends in the past, or has a reminder that is in the past.");
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
            await _eventScheduler.ScheduleEvent(savedEvent, ctx.Client, defaultChannelId, ctx.Guild.Id);

            var embed = EventEmbedFactory.GetCreateEventEmbed(savedEvent);
            await ctx.RespondAsync("New event created.", embed: embed);
        }

        [Command("list"), Description("Lists all events.")]
        [PermissionNode(PermissionNode.EventList)]
        public async Task List(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

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

            if (events.Count < 1)
            {
                await ctx.RespondAsync("No events found.");
                return;
            }

            var pages = EventListPageFactory.GetEventListPages(events);
            var interactivity = ctx.Client.GetInteractivity();
            await interactivity.SendPaginatedMessage(ctx.Channel, ctx.User, pages);
        }

        [Command("list"), Description("List the details of a single event.")]
        [PermissionNode(PermissionNode.EventList)]
        public async Task ListOne(CommandContext ctx, int index)
        {
            await ctx.TriggerTypingAsync();

            if (index <= 0)
            {
                await ctx.RespondAsync("Event index must be greater than 0.");
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
            catch (CalendarNotFoundException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }

            if (evt == null)
            {
                await ctx.RespondAsync("Event not found.");
                return;
            }

            var embed = EventEmbedFactory.GetViewEventEmbed(evt);
            await ctx.RespondAsync(embed: embed);
        }

        [Command("update"), Description("Update an event.")]
        [PermissionNode(PermissionNode.EventUpdate)]
        public async Task Update(CommandContext ctx, int index, [RemainingText] string args)
        {
            await ctx.TriggerTypingAsync();

            if (index <= 0)
            {
                await ctx.RespondAsync("Event index must be greater than 0.");
                return;
            }

            if (string.IsNullOrEmpty(args))
            {
                await ctx.RespondAsync("No arguments given for updating the event.");
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
            
            if (evt.HasStarted())
            {
                await ctx.RespondAsync("Cannot update an event that is in progress.");
                return;
            }

            Event updatedEvent;
            try
            {
                updatedEvent = EventParser.ParseUpdateEvent(evt, args, timezone);
            }
            catch (DateTimeInPastException)
            {
                await ctx.RespondAsync("Cannot create an event that starts or ends in the past, or has a reminder that is in the past.");
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
            savedEvent = await _eventService.UpdateEventAsync(evt);

            var defaultChannelId = await _calendarService.GetCalendarDefaultChannelAsync(ctx.Guild.Id);
            await _eventScheduler.RescheduleEvent(evt, ctx.Client, defaultChannelId);

            var embed = EventEmbedFactory.GetUpdateEventEmbed(savedEvent);
            await ctx.RespondAsync("Updated event.", embed: embed);
        }

        [Command("rsvp"), Description("Add or remove an RSVP to an event.")]
        [PermissionNode(PermissionNode.EventRSVP)]
        public async Task RSVP(CommandContext ctx, int index)
        {
            await ctx.TriggerTypingAsync();

            if (index <= 0)
            {
                await ctx.RespondAsync("Event index must be greater than 0.");
                return;
            }

            Event evt;
            try
            {
                evt = await _eventService.ToggleRSVPByIndexAsync(ctx.Guild.Id, ctx.Member.Id, index - 1);
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
            catch (ActiveEventException)
            {
                await ctx.RespondAsync("Cannot add or remove RSVP on an event already in progress.");
                return;
            }

            if (evt == null)
            {
                await ctx.RespondAsync("Event not found.");
                return;
            }

            var defaultChannelId = await _calendarService.GetCalendarDefaultChannelAsync(ctx.Guild.Id);
            await _eventScheduler.RescheduleEvent(evt, ctx.Client, defaultChannelId);

            if (evt.RSVPs.Any(r => r.UserId == ctx.Member.Id))
            {
                await ctx.RespondAsync($"Added RSVP for user {ctx.Member.GetUsernameAndDiscriminator()} for event {evt.Name}.");
            }
            else
            {
                await ctx.RespondAsync($"Removed RSVP for user {ctx.Member.GetUsernameAndDiscriminator()} for event {evt.Name}.");
            }
        }

        [Group("delete")]
        public class DeleteCommands : BaseCommandModule
        {
            private readonly IEventService _eventService;
            private readonly IPermissionService _permissionService;
            private readonly IEventScheduler _eventScheduler;

            public DeleteCommands(IEventService eventService, IPermissionService permissionService, IEventScheduler eventScheduler)
            {
                _eventService = eventService;
                _permissionService = permissionService;
                _eventScheduler = eventScheduler;
            }

            [GroupCommand, Description("Delete an event.")]
            [PermissionNode(PermissionNode.EventDelete)]
            public async Task Delete(CommandContext ctx, int index)
            {
                // Permission node check workaround for GroupCommand method
                var attr = (PermissionNodeAttribute)(
                    GetType()
                    .GetMethod("Delete")
                    .GetCustomAttributes(typeof(PermissionNodeAttribute), true)[0]);
                if (!(await attr.ExecuteCheckAsync(ctx, false)))
                {
                    return;
                }

                await ctx.TriggerTypingAsync();

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
                catch (EventNotFoundException)
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

            [Command("all"), Description("Delete all events.")]
            [PermissionNode(PermissionNode.EventDelete)]
            public async Task DeleteAll(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                List<Event> deletedEvents;
                try
                {
                    deletedEvents = await _eventService.DeleteAllEventsAsync(ctx.Guild.Id);
                }
                catch (CalendarNotFoundException)
                {
                    await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                    return;
                }
                
                foreach (var evt in deletedEvents)
                {
                    await _eventScheduler.UnscheduleEvent(evt);
                }

                await ctx.RespondAsync("Deleted all events.");
            }
        }
    }
}
