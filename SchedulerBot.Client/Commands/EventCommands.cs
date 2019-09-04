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
using MediatR;
using SchedulerBot.Application.Events.Commands.CreateEvent;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Events.Queries.GetEvents;
using SchedulerBot.Application.Events.Models;
using System.Text;
using System.Globalization;
using SchedulerBot.Application.Events.Queries.GetEvent;
using SchedulerBot.Application.Events.Commands.UpdateEvent;
using SchedulerBot.Application.Events.Commands.ToggleEventRsvp;
using SchedulerBot.Application.Events.Commands.DeleteEvent;

namespace SchedulerBot.Client.Commands
{
    [Group("event")]
    [Description("Commands for managing events.")]
    public class EventCommands : BotCommandModule
    {
        private readonly ICalendarService _calendarService;
        private readonly IEventService _eventService;
        private readonly IEventScheduler _eventScheduler;
        private readonly IEventParser _eventParser; // Temporary; remove when all methods are migrated to use mediator

        public EventCommands(IMediator mediator, ICalendarService calendarService, IEventService eventService, IEventScheduler eventScheduler, IEventParser eventParser) :
            base(mediator)
        {
            _calendarService = calendarService;
            _eventService = eventService;
            _eventScheduler = eventScheduler;
            _eventParser = eventParser;
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

            var command = new CreateEventCommand
            {
                CalendarId = ctx.Guild.Id,
                EventArgs = args
            };

            try
            {
                var result = await _mediator.Send(command);

                var defaultChannelId = await _calendarService.GetCalendarDefaultChannelAsync(ctx.Guild.Id);
                await _eventScheduler.ScheduleEvent(result, ctx.Client, defaultChannelId, ctx.Guild.Id);

                var embed = EventEmbedFactory.GetCreateEventEmbed(result);
                await ctx.RespondAsync("New event created.", embed: embed);
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
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
        }

        [Command("list"), Description("Lists all events.")]
        [PermissionNode(PermissionNode.EventList)]
        public async Task List(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var query = new GetEventsForCalendarQuery
            {
                CalendarId = ctx.Guild.Id
            };

            var events = await _mediator.Send(query);

            if (events.Count < 1)
            {
                await ctx.RespondAsync("No events found.");
                return;
            }

            var pages = GetEventListPages(events);
            var interactivity = ctx.Client.GetInteractivity();
            await interactivity.SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);
        }

        [Command("list"), Description("List the details of a single event.")]
        [PermissionNode(PermissionNode.EventList)]
        public async Task ListOne(CommandContext ctx, int index)
        {
            await ctx.TriggerTypingAsync();

            var query = new GetEventByIndexQuery
            {
                CalendarId = ctx.Guild.Id,
                Index = index - 1
            };
            var validator = new GetEventByIndexQueryValidator();
            var validationResult = validator.Validate(query);
            if (!validationResult.IsValid)
            {
                await ctx.RespondAsync("Event index must be greater than 0.");
                return;
            }

            try
            {
                var result = await _mediator.Send(query);

                var embed = EventEmbedFactory.GetViewEventEmbed(result);
                await ctx.RespondAsync(embed: embed);
            }
            catch (Application.Exceptions.EventNotFoundException)
            {
                await ctx.RespondAsync("Event not found.");
                return;
            }
        }

        [Command("update"), Description("Update an event.")]
        [PermissionNode(PermissionNode.EventUpdate)]
        public async Task Update(CommandContext ctx, int index, [RemainingText] string args)
        {
            await ctx.TriggerTypingAsync();

            if (string.IsNullOrEmpty(args))
            {
                await ctx.RespondAsync("No arguments given for updating the event.");
                return;
            }

            var command = new UpdateEventCommand
            {
                CalendarId = ctx.Guild.Id,
                EventArgs = args.Split(' '),
                EventIndex = index - 1
            };

            var validator = new UpdateEventCommandValidator();
            var validationResult = validator.Validate(command);
            if (!validationResult.IsValid)
            {
                await ctx.RespondAsync("Event index must be greater than 0.");
                return;
            }

            try
            {
                var result = await _mediator.Send(command);

                var defaultChannelId = await _calendarService.GetCalendarDefaultChannelAsync(ctx.Guild.Id);
                await _eventScheduler.RescheduleEvent(result, ctx.Client, defaultChannelId);

                var embed = EventEmbedFactory.GetUpdateEventEmbed(result);
                await ctx.RespondAsync("Updated event.", embed: embed);
            }
            catch (Application.Exceptions.EventNotFoundException)
            {
                await ctx.RespondAsync("Event not found.");
                return;
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
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
            catch (EventAlreadyStartedException)
            {
                await ctx.RespondAsync("Cannot update an event that is in progress.");
                return;
            }
        }

        [Command("rsvp"), Description("Add or remove an RSVP to an event.")]
        [PermissionNode(PermissionNode.EventRSVP)]
        public async Task RSVP(CommandContext ctx, int index)
        {
            await ctx.TriggerTypingAsync();

            var command = new ToggleEventRsvpCommand
            {
                CalendarId = ctx.Guild.Id,
                UserId = ctx.Member.Id,
                Index = index - 1
            };

            var validator = new ToggleEventRsvpCommandValidator();
            var validationResult = validator.Validate(command);
            if (!validationResult.IsValid)
            {
                await ctx.RespondAsync("Event index must be greater than 0.");
                return;
            }

            try
            {
                var result = await _mediator.Send(command);

                await _eventScheduler.RescheduleEvent(result, ctx.Client, result.DefaultChannel);

                if (result.RsvpAdded)
                {
                    await ctx.RespondAsync($"Added RSVP for user {ctx.Member.GetUsernameAndDiscriminator()} for event {result.Name}.");
                }
                else
                {
                    await ctx.RespondAsync($"Removed RSVP for user {ctx.Member.GetUsernameAndDiscriminator()} for event {result.Name}.");
                }
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
            catch (Application.Exceptions.EventNotFoundException)
            {
                await ctx.RespondAsync("Event not found.");
                return;
            }
            catch (EventAlreadyStartedException)
            {
                await ctx.RespondAsync("Cannot add or remove RSVP on an event already in progress.");
                return;
            }
        }

        [Group("delete")]
        public class DeleteCommands : BotCommandModule
        {
            private readonly IEventService _eventService;
            private readonly IEventScheduler _eventScheduler;

            public DeleteCommands(IMediator mediator, IEventService eventService, IEventScheduler eventScheduler) :
                base(mediator)
            {
                _eventService = eventService;
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

                var command = new DeleteEventByIndexCommand
                {
                    CalendarId = ctx.Guild.Id,
                    Index = index - 1
                };

                var validator = new DeleteEventByIndexCommandValidator();
                var validationResult = validator.Validate(command);
                if (!validationResult.IsValid)
                {
                    await ctx.RespondAsync("Event index must be greater than 0.");
                    return;
                }

                try
                {
                    var result = await _mediator.Send(command);

                    await _eventScheduler.UnscheduleEvent(result);

                    var embed = EventEmbedFactory.GetDeleteEventEmbed(result);
                    await ctx.RespondAsync("Deleted event.", embed: embed);
                }
                catch (Application.Exceptions.EventNotFoundException)
                {
                    await ctx.RespondAsync("Event not found.");
                    return;
                }
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

        private List<Page> GetEventListPages(List<SimplifiedEventViewModel> events)
        {
            StringBuilder sb = new StringBuilder();
            List<Page> pages = new List<Page>();
            ushort i = 0;
            bool activeEventHeaderWritten = false;
            bool upcomingEventHeaderWritten = false;
            DateTimeOffset now = DateTimeOffset.Now;

            foreach (var evt in events)
            {
                if (i % 10 == 0)
                {
                    if (i != 0)
                    {
                        sb.AppendLine("```");
                        sb.AppendLine("Run `event list <event number>` to view details for a certain event.");
                        pages.Add(new Page(sb.ToString()));
                        activeEventHeaderWritten = false;
                        upcomingEventHeaderWritten = false;
                    }

                    sb = new StringBuilder();
                    sb.AppendLine("```css");
                }

                if (evt.StartTimestamp <= now && !activeEventHeaderWritten)
                {
                    sb.AppendLine("[Active Events]");
                    sb.AppendLine();
                    activeEventHeaderWritten = true;
                }
                else if (evt.StartTimestamp > now && !upcomingEventHeaderWritten)
                {
                    sb.AppendLine();
                    sb.AppendLine("[Upcoming Events]");
                    sb.AppendLine();
                    upcomingEventHeaderWritten = true;
                }
                sb.AppendLine($"{i + 1}: {evt.Name} /* {evt.StartTimestamp.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture)} to {evt.EndTimestamp.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture)} */");

                i++;
            }

            // Push last page to list
            sb.AppendLine("```");
            sb.AppendLine("Run `event list <event number>` to view details for a certain event.");
            pages.Add(new Page(sb.ToString()));

            return pages;
        }
    }
}
