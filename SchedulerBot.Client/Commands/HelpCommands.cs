using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using SchedulerBot.Client.Builders;

namespace SchedulerBot.Client.Commands
{
    [Group("help")]
    public class HelpCommands : BaseCommandModule
    {
        private readonly IConfigurationRoot _configuration;

        public HelpCommands(IConfigurationRoot configuration) => _configuration = configuration;

        [GroupCommand]
        public async Task Help(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(211, 255, 219),
                Title = "Help",
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = "SchedulerBot",
                    IconUrl = "https://cdn.discordapp.com/avatars/339019867325726722/e5fca7dbae7156e05c013766fa498fe1.png"
                },
                Description = "All possible main commands are listed below.",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "To get additional help for a specific command, run \"help <command>\"."
                }
            };

            embed.AddField("init", "Initialises the guild calendar with a specific timezone.")
                .AddField("event", "Create, update and delete events, and RSVP to events to indicate attendance.")
                .AddField("perms", "Set user- or role-specific command permissions.")
                .AddField("settings", "View or change the bot's timezone, default channel and/or prefix settings.")
                .AddField("prefix", "View the bot's current prefix.")
                .AddField("ping", "Pings the bot.")
                .AddField("info", "Get information about the bot.")
                .AddField("time", "Get the current time in the set timezone.")
                .AddField("Need more help? Did you find an issue with the bot?", "Run the `support` command to get an invite link to the support server and get in touch with the developer.")
                .AddField("Want to add the bot to your own server?", "Run the `invite` command to get an invite link for the bot.");

            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("init")]
        public async Task Init(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var timezoneLink = _configuration.GetSection("Bot").GetSection("Links").GetValue<string>("TimezoneList");

            var usageOptions = new Dictionary<string, string>
            {
                ["<timezone>"] = $"A timezone name from the tz database (case-sensitive). See {timezoneLink} under the TZ column for a list of valid timezones"
            };

            var examples = new List<string>
            {
                "init America/Los_Angeles",
                "init GMT"
            };

            var embed = new HelpEmbedBuilder()
                .WithCommandString("init")
                .WithDescription("Command to initialise the guild calendar with a specific timezone. This command **must** be run first before creating events.")
                .WithUsage("init <timezone>", usageOptions)
                .WithExamples(examples)
                .Build();

            await ctx.RespondAsync(embed: embed);
        }

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new HelpEmbedBuilder()
                .WithCommandString("ping")
                .WithDescription("Pings the bot.")
                .WithUsage("ping")
                .Build();

            await ctx.RespondAsync(embed: embed);
        }

        [Command("prefix")]
        public async Task Prefix(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new HelpEmbedBuilder()
                .WithCommandString("prefix")
                .WithDescription("View the bot's prefix.")
                .WithUsage("prefix")
                .Build();

            await ctx.RespondAsync(embed: embed);
        }

        [Command("info")]
        public async Task Info(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new HelpEmbedBuilder()
                .WithCommandString("info")
                .WithDescription("Get information about the bot.")
                .WithUsage("info")
                .Build();

            await ctx.RespondAsync(embed: embed);
        }

        [Command("support")]
        public async Task Support(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new HelpEmbedBuilder()
                .WithCommandString("support")
                .WithDescription("Get an invite link to the bot's support server.")
                .WithUsage("support")
                .Build();

            await ctx.RespondAsync(embed: embed);
        }
        
        [Command("time")]
        public async Task Time(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new HelpEmbedBuilder()
                .WithCommandString("time")
                .WithDescription("View the current time in the set timezone.")
                .WithUsage("time")
                .Build();

            await ctx.RespondAsync(embed: embed);
        }

        [Command("invite")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var embed = new HelpEmbedBuilder()
                .WithCommandString("invite")
                .WithDescription("Get an invite link to invite the bot to a server.")
                .WithUsage("invite")
                .Build();

            await ctx.RespondAsync(embed: embed);
        }

        [Group("event")]
        public class EventHelpCommands : BaseCommandModule
        {
            [GroupCommand]
            public async Task Help(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var usageOptions = new Dictionary<string, string>
                {
                    ["<event details>"] = "Name and date/time of the event in natural language"
                };

                var options = new Dictionary<string, string>
                {
                    ["--desc <description>"] = "Set event description",
                    ["--repeat d|w|m|mw"] = "Set event to repeat daily/weekly/monthly/monthlyweekday",
                    ["--mention <mentions>"] = "Add users/roles to mention when the event starts",
                    ["--remind <period>"] = "Set a reminder that will trigger a certain period of time before the event starts"
                };

                var examples = new List<string>
                {
                    "event Scrims tomorrow 8pm-9pm",
                    "event Weekly Raid Night 20 August 10.30" +
                    "pm --repeat w --remind 15 minutes",
                    "event Zero Hour 15 Jan 10am to 20 Jan 10pm --desc In-game event",
                    "event Team Practice 6pm --repeat d --mention @AlphaTeam"
                };

                var availableSubcommands = new List<string>
                {
                    "list",
                    "update",
                    "delete",
                    "rsvp"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("event")
                    .WithDescription("Create an event, and be notified when the event is starting.")
                    .WithUsage("event <event details> [<options>]", usageOptions)
                    .WithOptions(options)
                    .WithExamples(examples)
                    .WithAvailableSubcommands(availableSubcommands)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }

            [Command("list")]
            public async Task List(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var usageOptions = new Dictionary<string, string>
                {
                    ["[event number]"] = "Number of the event to view"
                };

                var examples = new List<string>
                {
                    "event list",
                    "event list 1"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("event list")
                    .WithDescription("View a list of created events, or the details for a single event.")
                    .WithUsage("event list [event number]", usageOptions)
                    .WithExamples(examples)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }

            [Command("update")]
            public async Task Update(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var usageOptions = new Dictionary<string, string>
                {
                    ["<event number>"] = "Number of the event to update"
                };

                var options = new Dictionary<string, string>
                {
                    ["<event details>"] = "Name and date/time of the event in natural language",
                    ["--desc <description>"] = "Set event description",
                    ["--repeat d|w|m|mw"] = "Set event to repeat daily/weekly/monthly/monthlyweekday",
                    ["--mention <mentions>"] = "Add users/roles to mention when the event starts",
                    ["--remind <period>"] = "Set a reminder that will trigger a certain period of time before the event starts"
                };

                var examples = new List<string>
                {
                    "event update 1 Scrims tomorrow 8pm-9pm",
                    "event update 2 --repeat w --remind 15 minutes",
                    "event update 3 --desc In-game event",
                    "event update 4 --mention @AlphaTeam"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("event update")
                    .WithDescription("Update an existing event.")
                    .WithUsage("event update <event number> [<options>]", usageOptions)
                    .WithOptions(options)
                    .WithExamples(examples)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }

            [Command("delete")]
            public async Task Delete(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var usageOptions = new Dictionary<string, string>
                {
                    ["<event number>"] = "Number of the event to delete"
                };

                var examples = new List<string>
                {
                    "event delete 1",
                    "event delete all"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("event delete")
                    .WithDescription("Delete an event, or all events.")
                    .WithUsage("event delete <event number> | all", usageOptions)
                    .WithExamples(examples)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }

            [Command("rsvp")]
            public async Task Rsvp(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var usageOptions = new Dictionary<string, string>
                {
                    ["<event number>"] = "Number of the event to RSVP to"
                };

                var examples = new List<string>
                {
                    "event rsvp 1"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("event rsvp")
                    .WithDescription("Toggle own RSVP for a certain event.")
                    .WithUsage("event rsvp <event number>", usageOptions)
                    .WithExamples(examples)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }
        }

        [Group("perms")]
        public class PermissionHelpCommands : BaseCommandModule
        {
            [GroupCommand]
            public async Task Help(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var availableSubcommands = new List<string>
                {
                    "allow",
                    "deny",
                    "show",
                    "nodes"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("perms")
                    .WithDescription("Modify permissions for certain commands by user and/or role.")
                    .WithAvailableSubcommands(availableSubcommands)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }

            [Command("deny")]
            public async Task Deny(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var usageOptions = new Dictionary<string, string>
                {
                    ["<permission node>"] = "The permission node to deny usage for",
                    ["<role mention>"] = "A mention for the role to deny permissions for",
                    ["<user mention>"] = "A mention for the user to deny permissions for"
                };

                var examples = new List<string>
                {
                    "perms deny EventCreate @SomeUser",
                    "perms deny EventRSVP @SomeRole",
                    "perms deny PermsModify @everyone"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("perms deny")
                    .WithDescription("Deny permissions for a certain user, role or everyone.")
                    .WithUsage("perms deny <permission node> <role mention>|<user mention>|@everyone", usageOptions)
                    .WithExamples(examples)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }

            [Command("allow")]
            public async Task Allow(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var usageOptions = new Dictionary<string, string>
                {
                    ["<permission node>"] = "The permission node to allow usage for",
                    ["<role mention>"] = "A mention for the role to allow permissions for",
                    ["<user mention>"] = "A mention for the user to allow permissions for"
                };

                var examples = new List<string>
                {
                    "perms allow EventCreate @SomeUser",
                    "perms allow EventRSVP @SomeRole",
                    "perms allow PermsModify @everyone"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("perms allow")
                    .WithDescription("Allow permissions for a certain user, role or everyone.")
                    .WithUsage("perms allow <permission node> <role mention>|<user mention>|@everyone", usageOptions)
                    .WithExamples(examples)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }

            [Command("show")]
            public async Task Show(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var usageOptions = new Dictionary<string, string>
                {
                    ["<permission node>"] = "The permission node to show denied permissions for",
                    ["<role mention>"] = "A mention for the role to show denied permissions for",
                    ["<user mention>"] = "A mention for the user to show denied permissions for"
                };

                var examples = new List<string>
                {
                    "perms show EventList",
                    "perms show @SomeUser",
                    "perms show @SomeRole",
                    "perms show @everyone"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("perms show")
                    .WithDescription("Show denied permissions for a certain permission node, user, role or everyone.")
                    .WithUsage("perms show <permission node>|<role mention>|<user mention>|@everyone", usageOptions)
                    .WithExamples(examples)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }

            [Command("nodes")]
            public async Task Nodes(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("perms nodes")
                    .WithDescription("List all available permission nodes.")
                    .WithUsage("perms nodes")
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }
        }

        [Group("settings")]
        public class SettingsHelpCommands : BaseCommandModule
        {
            private readonly IConfigurationRoot _configuration;

            public SettingsHelpCommands(IConfigurationRoot configuration) => _configuration = configuration;
            
            [GroupCommand]
            public async Task Help(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var availableSubcommands = new List<string>
                {
                    "timezone",
                    "defaultchannel",
                    "prefix"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("settings")
                    .WithDescription("View settings for the bot.")
                    .WithUsage("settings")
                    .WithAvailableSubcommands(availableSubcommands)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }

            [Command("timezone")]
            public async Task Timezone(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var timezoneLink = _configuration.GetSection("Bot").GetSection("Links").GetValue<string>("TimezoneList");

                var usageOptions = new Dictionary<string, string>
                {
                    ["[new timezone]"] = $"A timezone name from the tz database (case-sensitive). See {timezoneLink} under the TZ column for a list of valid timezones"
                };

                var examples = new List<string>
                {
                    "settings timezone",
                    "settings timezone America/Los_Angeles"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("settings timezone")
                    .WithDescription("View or set the bot's timezone.")
                    .WithUsage("settings timezone [new timezone]", usageOptions)
                    .WithExamples(examples)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }

            [Command("defaultchannel")]
            public async Task DefaultChannel(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();

                var usageOptions = new Dictionary<string, string>
                {
                    ["[new channel]"] = "A channel mention for a new default channel"
                };

                var examples = new List<string>
                {
                    "settings defaultchannel",
                    "settings defaultchannel #some-channel"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("settings defaultchannel")
                    .WithDescription("View or set the bot's default channel, which the bot sends event notifications to.")
                    .WithUsage("settings defaultchannel [new channel]", usageOptions)
                    .WithExamples(examples)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }

            [Command("prefix")]
            public async Task Prefix(CommandContext ctx)
            {
                await ctx.TriggerTypingAsync();
                var usageOptions = new Dictionary<string, string>
                {
                    ["[new prefix]"] = "A prefix string for the new prefix"
                };

                var examples = new List<string>
                {
                    "settings prefix",
                    "settings prefix ++"
                };

                var embed = new HelpEmbedBuilder()
                    .WithCommandString("settings prefix")
                    .WithDescription("View or set the bot's prefix.")
                    .WithUsage("settings prefix [new prefix]", usageOptions)
                    .WithExamples(examples)
                    .Build();

                await ctx.RespondAsync(embed: embed);
            }
        }
    }
}
