using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MediatR;
using Microsoft.Extensions.Configuration;
using NodaTime;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Settings.Queries.GetSetting;
using SchedulerBot.Client.Attributes;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Services;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Commands
{
    public class MiscCommands : BotCommandModule
    {
        private readonly ICalendarService _calendarService;
        private readonly IShardedClientInformationService _shardedClientInformationService;
        internal readonly IPermissionService _permissionService;
        private readonly IConfigurationRoot _configuration;

        public MiscCommands(IMediator mediator, ICalendarService calendarService, IShardedClientInformationService shardedClientInformationService, IPermissionService permissionService, IConfigurationRoot configuration) :
            base(mediator)
        {
            _calendarService = calendarService;
            _shardedClientInformationService = shardedClientInformationService;
            _permissionService = permissionService;
            _configuration = configuration;
        }

        [Command("ping"), Description("Pings the bot.")]
        [PermissionNode(PermissionNode.Ping)]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            TimeSpan diff = DateTimeOffset.Now - ctx.Message.Timestamp;
            await ctx.RespondAsync($"Pong! Time: {diff.Milliseconds}ms");
        }

        [Command("prefix"), Description("View the bot's current prefix for your guild.")]
        [PermissionNode(PermissionNode.PrefixShow)]
        public async Task Prefix(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            try
            {
                var result = await _mediator.Send(new GetPrefixSettingQuery
                {
                    CalendarId = ctx.Guild.Id
                });

                await ctx.RespondAsync($"`{result.Prefix}`");
            }
            catch (CalendarNotInitialisedException)
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }
        }

        [Command("info"), Description("Get some information about the bot.")]
        public async Task Info(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            var uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = "SchedulerBot",
                    IconUrl = "https://cdn.discordapp.com/avatars/339019867325726722/e5fca7dbae7156e05c013766fa498fe1.png"
                },
                Color = new DiscordColor(211, 255, 219),
                Description = "A Discord bot for scheduling events.",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Powered by the DSharpPlus library (https://dsharpplus.emzi0767.com/)"
                }
            };
            embed.AddField("Version", version, true);
            embed.AddField("Guilds", _shardedClientInformationService.GetTotalGuildCount().ToString(), true);
            embed.AddField("Users", _shardedClientInformationService.GetTotalUserCount().ToString(), true);
            embed.AddField("Shard Number", $"{ctx.Client.ShardId + 1}/{ctx.Client.ShardCount}", true);
            embed.AddField("Uptime", $"{uptime.Days} day(s), {uptime.Hours} hour(s), {uptime.Minutes} minute(s), {uptime.Seconds} second(s)");
            embed.AddField("Like the bot?", "[Support us on Patreon!](https://patreon.com/SchedulerBot)");

            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("support"), Description("Get an invite link to the SchedulerBot support server.")]
        public async Task Support(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var supportLink = _configuration.GetSection("Bot").GetSection("Links").GetValue<string>("SupportServer");
            await ctx.RespondAsync($"Click the following link to join the bot's support server. {supportLink}");
        }

        [Command("invite"), Description("Get a link to invite the bot to your server.")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var inviteLink = _configuration.GetSection("Bot").GetSection("Links").GetValue<string>("BotInvite");
            await ctx.RespondAsync($"Click the following link to invite the bot to your server. {inviteLink}");
        }

        [Command("time"), Description("Gets the current time according to the set timezone.")]
        [PermissionNode(PermissionNode.Time)]
        public async Task Time(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var timezone = await _calendarService.GetCalendarTimezoneAsync(ctx.Guild.Id);
            if (string.IsNullOrEmpty(timezone))
            {
                await ctx.RespondAsync("Calendar not initialised. Run `init <timezone>` to initialise the calendar.");
                return;
            }

            var tz = DateTimeZoneProviders.Tzdb[timezone];
            Instant now = SystemClock.Instance.GetCurrentInstant();
            var dateTimeNow = now.InZone(tz).ToDateTimeOffset();

            var sb = new StringBuilder();
            sb.AppendLine($"Timezone: {timezone}");
            sb.AppendLine(dateTimeNow.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture));
            await ctx.RespondAsync(sb.ToString());
        }
    }
}
