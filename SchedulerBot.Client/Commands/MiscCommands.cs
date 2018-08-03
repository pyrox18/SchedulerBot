using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SchedulerBot.Client.Attributes;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;

namespace SchedulerBot.Client.Commands
{
    public class MiscCommands : BaseCommandModule
    {
        private readonly ICalendarService _calendarService;
        internal readonly IPermissionService _permissionService;

        public MiscCommands(ICalendarService calendarService, IPermissionService permissionService)
        {
            _calendarService = calendarService;
            _permissionService = permissionService;
        }

        [Command("ping"), Description("Pings the bot.")]
        [PermissionNode(PermissionNode.Ping)]
        public async Task Ping(CommandContext ctx)
        {
            if (!await this.CheckPermission(_permissionService, typeof(MiscCommands), nameof(MiscCommands.Ping), ctx.Member))
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
                return;
            }

            TimeSpan diff = DateTimeOffset.Now - ctx.Message.Timestamp;
            await ctx.RespondAsync($"Pong! Time: {diff.Milliseconds}ms");
        }

        [Command("prefix"), Description("View the bot's current prefix for your guild.")]
        [PermissionNode(PermissionNode.PrefixShow)]
        public async Task Prefix(CommandContext ctx)
        {
            if (!await this.CheckPermission(_permissionService, typeof(MiscCommands), nameof(MiscCommands.Prefix), ctx.Member))
            {
                await ctx.RespondAsync("You are not permitted to use this command.");
                return;
            }

            var prefix = await _calendarService.GetCalendarPrefixAsync(ctx.Guild.Id);
            await ctx.RespondAsync($"`{prefix}`");
        }

        [Command("info"), Description("Get some information about the bot.")]
        public async Task Info(CommandContext ctx)
        {
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
            embed.AddField("Guilds", ctx.Client.Guilds.Count.ToString(), true);
            embed.AddField("Uptime", $"{uptime.Days} day(s), {uptime.Hours} hour(s), {uptime.Minutes} minute(s), {uptime.Seconds} second(s)");

            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("support"), Description("Get an invite link to the SchedulerBot support server.")]
        public async Task Support(CommandContext ctx)
        {
            await ctx.RespondAsync("Click the following link to join the bot's support server. https://discord.gg/CRxRn5X");
        }

        [Command("invite"), Description("Get a link to invite the bot to your server.")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.RespondAsync("Click the following link to invite the bot to your server. https://goo.gl/E7hLK9");
        }
    }
}
