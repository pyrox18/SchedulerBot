using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SchedulerBot.Application.Calendars.Commands.CreateCalendar;
using SchedulerBot.Application.Calendars.Commands.DeleteCalendar;
using SchedulerBot.Application.Events.Commands.CleanPastEvents;
using SchedulerBot.Application.Exceptions;
using SchedulerBot.Application.Permissions.Commands.DeleteRolePermissions;
using SchedulerBot.Application.Permissions.Commands.DeleteUserPermissions;
using SchedulerBot.Application.Settings.Queries.GetSetting;
using SchedulerBot.Client.Attributes;
using SchedulerBot.Client.Commands;
using SchedulerBot.Client.Configuration;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Services;
using SharpRaven;
using SharpRaven.Data;

namespace SchedulerBot.Client
{
    public class Bot : IHostedService
    {
        private readonly ILogger<Bot> _logger;
        private readonly DiscordShardedClient _shardedClient;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMediator _mediator;
        private readonly IMemoryCache _cache;
        private readonly IErrorHandlerService _errorHandlerService;
        private readonly BotConfiguration _configuration;
        private readonly string _version;

        public Bot(
            ILogger<Bot> logger,
            DiscordShardedClient shardedClient,
            IServiceProvider serviceProvider,
            IMemoryCache cache,
            IMediator mediator,
            IOptions<BotConfiguration> configuration,
            IErrorHandlerService errorHandlerService)
        {
            _logger = logger;
            _shardedClient = shardedClient;
            _serviceProvider = serviceProvider;
            _cache = cache;
            _mediator = mediator;
            _configuration = configuration.Value;
            _errorHandlerService = errorHandlerService;

            _version = Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"SchedulerBot v{_version}");

            // Apply deletes and repeats to events that have ended
            _logger.LogInformation("Deleting and repeating past events");
            await _mediator.Send(new CleanPastEventsCommand());

            _logger.LogInformation("Setting up client interactivity module");
            await _shardedClient.UseInteractivityAsync(new InteractivityConfiguration());
            _shardedClient.DebugLogger.LogMessageReceived += OnLogMessageReceived;

            _logger.LogInformation("Initialising command module");
            var commandsNextExtensions = await _shardedClient.UseCommandsNextAsync(new CommandsNextConfiguration
            {
                PrefixResolver = ResolvePrefix,
                Services = _serviceProvider,
                EnableDefaultHelp = false
            });

            _logger.LogInformation("Registering command extensions");
            foreach (var ext in commandsNextExtensions)
            {
                var commands = ext.Value;
                commands.RegisterCommands(typeof(AdminCommands).Assembly);

                commands.CommandErrored += _errorHandlerService.HandleCommandErrorAsync;
            }

            // Register event handlers
            _logger.LogInformation("Registering event handlers");
            _shardedClient.GuildCreated += OnGuildCreate;
            _shardedClient.GuildDeleted += OnGuildDelete;
            _shardedClient.GuildMemberRemoved += OnGuildMemberRemove;
            _shardedClient.GuildRoleDeleted += OnGuildRoleDelete;
            _shardedClient.Ready += OnClientReady;

            _logger.LogInformation("Connecting all shards...");
            await _shardedClient.StartAsync();
            _logger.LogInformation("All shards connected");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task<int> ResolvePrefix(DiscordMessage msg)
        {
            if (!_cache.TryGetValue($"prefix:{msg.Channel.GuildId}", out string prefix))
            {
                try
                {
                    var result = await _mediator.Send(new GetPrefixSettingQuery
                    {
                        CalendarId = msg.Channel.GuildId
                    });

                    prefix = result.Prefix;
                }
                catch (CalendarNotInitialisedException)
                {
                    prefix = _configuration.Prefixes[0];
                }

                // Store prefix in cache
                _cache.Set($"prefix:{msg.Channel.GuildId}", prefix, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(3)));
            }

            if (!msg.Content.StartsWith(prefix))
            {
                return -1;
            }

            return prefix.Length;
        }

        private async Task OnGuildCreate(GuildCreateEventArgs e)
        {
            await _mediator.Send(new CreateCalendarCommand
            {
                CalendarId = e.Guild.Id,
                Prefix = _configuration.Prefixes[0]
            });
        }

        private async Task OnGuildDelete(GuildDeleteEventArgs e)
        {
            await _mediator.Send(new DeleteCalendarCommand
            {
                CalendarId = e.Guild.Id
            });
        }

        private async Task OnGuildMemberRemove(GuildMemberRemoveEventArgs e)
        {
            await _mediator.Send(new DeleteUserPermissionsCommand
            {
                CalendarId = e.Guild.Id,
                UserId = e.Member.Id
            });
        }

        private async Task OnGuildRoleDelete(GuildRoleDeleteEventArgs e)
        {
            await _mediator.Send(new DeleteRolePermissionsCommand
            {
                CalendarId = e.Guild.Id,
                RoleId = e.Role.Id
            });
        }

        private async Task OnClientReady(ReadyEventArgs e)
        {
            // Set status
            _logger.LogInformation("Updating status");
            await e.Client.UpdateStatusAsync(new DiscordActivity(string.Format(_configuration.Status, _version)));
        }

        private async Task OnCommandError(CommandErrorEventArgs e)
        {
            var exceptionType = e.Exception.GetType();

            if (exceptionType == typeof(ChecksFailedException)
                && (e.Exception as ChecksFailedException).FailedChecks.Any(x => x.GetType() == typeof(PermissionNodeAttribute)))
            {
                return;
            }

            if (exceptionType != typeof(CommandNotFoundException) && exceptionType != typeof(ArgumentException) && exceptionType != typeof(UnauthorizedException) && exceptionType != typeof(InvalidOperationException))
            {
                var errorId = Guid.NewGuid();
                _logger.LogError($"{errorId}: {e.Exception.Message}\n{e.Exception.StackTrace}");

                string sentryEventId = string.Empty;
                IRavenClient ravenClient = (IRavenClient)_serviceProvider.GetService(typeof(IRavenClient));
                if (ravenClient != null)
                {
                    e.Exception.Data.Add("ErrorEventId", errorId.ToString());
                    e.Exception.Data.Add("Message", e.Context.Message);
                    e.Exception.Data.Add("Command", e.Command.QualifiedName);
                    e.Exception.Data.Add("User", e.Context.Member.GetUsernameAndDiscriminator());
                    e.Exception.Data.Add("UserId", e.Context.Member.Id);
                    e.Exception.Data.Add("ShardId", e.Context.Client.ShardId);

                    sentryEventId = await ravenClient.CaptureAsync(new SentryEvent(e.Exception));
                }

                var sb = new StringBuilder();
                sb.AppendLine("An error has occurred. Please report this in the support server using the `support` command.");
                sb.AppendLine($"Error event ID: {errorId}");
                if (!string.IsNullOrEmpty(sentryEventId))
                {
                    sb.AppendLine($"Sentry event ID: {sentryEventId}");
                }
                sb.AppendLine("```");
                sb.AppendLine($"{e.Exception.Message}");
                sb.AppendLine("```");
                await e.Context.RespondAsync(sb.ToString());
            }
        }

        private void OnLogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            switch (e.Level)
            {
                case DSharpPlus.LogLevel.Critical:
                    _logger.LogCritical($"[{e.Application}] {e.Message}");
                    break;
                case DSharpPlus.LogLevel.Debug:
                    _logger.LogDebug($"[{e.Application}] {e.Message}");
                    break;
                case DSharpPlus.LogLevel.Error:
                    _logger.LogError($"[{e.Application}] {e.Message}");
                    break;
                case DSharpPlus.LogLevel.Info:
                    _logger.LogInformation($"[{e.Application}] {e.Message}");
                    break;
                case DSharpPlus.LogLevel.Warning:
                    _logger.LogWarning($"[{e.Application}] {e.Message}");
                    break;
            }
        }
    }
}
