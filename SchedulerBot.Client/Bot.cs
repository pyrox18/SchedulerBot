using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SchedulerBot.Application.Calendars.Commands.InitialiseCalendar;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Client.Attributes;
using SchedulerBot.Client.Commands;
using SchedulerBot.Client.Configuration;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Scheduler;
using SchedulerBot.Client.Services;
using SchedulerBot.Data;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;
using SchedulerBot.Persistence;
using SchedulerBot.Persistence.Repositories;
using SharpRaven;
using SharpRaven.Data;

namespace SchedulerBot.Client
{
    public class Bot
    {
        private readonly IConfigurationRoot _configuration;
        private readonly DiscordShardedClient _client;
        private readonly IServiceProvider _serviceProvider;

        public Bot()
        {
            // Build configuration
            var builder = new ConfigurationBuilder();

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(environment))
            {
                throw new ArgumentNullException("Environment not found in ASPNETCORE_ENVIRONMENT");
            }

            Console.WriteLine($"Environment: {environment}");

            builder.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            builder.AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", true);

            builder.AddEnvironmentVariables();
            _configuration = builder.Build();

            // Configure service provider
            Console.WriteLine("Configuring services...");
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            // Configure services
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            Configure(loggerFactory);

            // Initialise bot client
            var configBuilder = new DiscordConfigurationBuilder();
            var config = configBuilder.WithToken(_configuration.GetSection("Bot").GetValue<string>("Token"))
                .WithLogLevel(_configuration.GetSection("Logging").GetSection("LogLevel").GetValue<string>("Default"))
                .Build();
            _client = new DiscordShardedClient(config);
        }
        public async Task Run()
        {
            var logger = _serviceProvider.GetService<ILogger<Program>>();
            var version = Assembly.GetEntryAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;

            // Bot
            logger.LogInformation($"SchedulerBot v{version}");

            // Apply deletes and repeats to events that have ended
            var eventService = _serviceProvider.GetService<IEventService>();
            logger.LogInformation("Deleting and repeating past events");
            await eventService.ApplyDeleteAndRepeatPastEventsAsync();

            logger.LogInformation("Setting up client");
            await _client.UseInteractivityAsync(new InteractivityConfiguration());
            _client.DebugLogger.LogMessageReceived += OnLogMessageReceived;

            logger.LogInformation("Initialising command module");
            var commandsNextExtensions = await _client.UseCommandsNextAsync(new CommandsNextConfiguration
            {
                PrefixResolver = ResolvePrefix,
                Services = _serviceProvider,
                EnableDefaultHelp = false
            });

            logger.LogInformation("Registering command extensions");
            foreach (var ext in commandsNextExtensions)
            {
                var commands = ext.Value;
                commands.RegisterCommands<AdminCommands>();
                commands.RegisterCommands<EventCommands>();
                commands.RegisterCommands<InitializerCommands>();
                commands.RegisterCommands<MiscCommands>();
                commands.RegisterCommands<PermissionsCommands>();
                commands.RegisterCommands<SettingsCommands>();
                commands.RegisterCommands<HelpCommands>();

                commands.CommandErrored += OnCommandError;
            }

            // Register event handlers
            logger.LogInformation("Registering event handlers");
            _client.GuildCreated += OnGuildCreate;
            _client.GuildDeleted += OnGuildDelete;
            _client.GuildMemberRemoved += OnGuildMemberRemove;
            _client.GuildRoleDeleted += OnGuildRoleDelete;
            _client.Ready += OnClientReady;

            // Start event scheduler
            var scheduler = _serviceProvider.GetService<IEventScheduler>();
            logger.LogInformation("Starting event scheduler");
            await scheduler.Start();

            logger.LogInformation("Connecting all shards...");
            await _client.StartAsync();
            logger.LogInformation("All shards connected");

            await Task.Delay(-1);

            await scheduler.Shutdown();
            NLog.LogManager.Shutdown();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var connectionString = _configuration.GetConnectionString("SchedulerBotContext");
            var connectionString2 = _configuration.GetConnectionString("SchedulerBot");

            var loggerFactory = new LoggerFactory();
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(options =>
            {
                var logLevel = Enum.Parse<Microsoft.Extensions.Logging.LogLevel>(_configuration.GetSection("Logging").GetSection("LogLevel").GetValue<string>("Default"));
                options.SetMinimumLevel(logLevel);
            });

            // Add configuration as a service
            services.AddSingleton(_configuration);

            // Add cache service for caching prefixes
            services.AddMemoryCache();

            // Add Raven client as a service for production environment
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                var dsn = _configuration.GetSection("Raven").GetValue<string>("DSN");
                var ravenClient = new RavenClient(dsn);
                services.AddSingleton<IRavenClient>(ravenClient);
            }

            // Configure database
            services.AddSingleton(new SchedulerBotContextFactory(connectionString));
            services.AddDbContextPool<SchedulerBotDbContext>(options =>
            {
                options.UseNpgsql(connectionString2);
            }, 5);

            services.AddSingleton<ICalendarService, CalendarService>()
                .AddSingleton<IEventService, EventService>()
                .AddSingleton<IPermissionService, PermissionService>()
                .AddSingleton<IShardedClientInformationService, ShardedClientInformationService>(s => new ShardedClientInformationService(_client));

            // Repositories
            services.AddScoped<ICalendarRepository, CalendarRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();

            // MediatR
            services.AddMediatR(typeof(InitialiseCalendarCommand).Assembly);

            // Scheduler service
            services.AddSingleton<IEventScheduler, EventScheduler>();
        }

        private void Configure(ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog(new NLogProviderOptions
            {
                CaptureMessageProperties = true,
                CaptureMessageTemplates = true
            });
            NLog.LogManager.LoadConfiguration("nlog.config");
        }

        private async Task OnGuildCreate(GuildCreateEventArgs e)
        {
            var calendar = new Calendar
            {
                Id = e.Guild.Id,
                Events = new List<Event>(),
                Prefix = _configuration.GetSection("Bot").GetSection("Prefixes").Get<string[]>()[0]
            };

            var calendarService = _serviceProvider.GetService<ICalendarService>();
            await calendarService.CreateCalendarAsync(calendar);
        }

        private async Task OnGuildDelete(GuildDeleteEventArgs e)
        {
            var calendarService = _serviceProvider.GetService<ICalendarService>();
            await calendarService.DeleteCalendarAsync(e.Guild.Id);
        }

        private async Task OnGuildMemberRemove(GuildMemberRemoveEventArgs e)
        {
            var permissionService = _serviceProvider.GetService<IPermissionService>();
            await permissionService.RemoveUserPermissionsAsync(e.Guild.Id, e.Member.Id);
        }

        private async Task OnGuildRoleDelete(GuildRoleDeleteEventArgs e)
        {
            var permissionService = _serviceProvider.GetService<IPermissionService>();
            await permissionService.RemoveRolePermissionsAsync(e.Guild.Id, e.Role.Id);
        }

        private async Task OnClientReady(ReadyEventArgs e)
        {
            var logger = _serviceProvider.GetService<ILogger<Program>>();

            // Set status
            logger.LogInformation("Updating status");
            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            await e.Client.UpdateStatusAsync(new DiscordActivity(string.Format(_configuration.GetSection("Bot").GetValue<string>("Status"), version)));

            // Start event polling
            logger.LogInformation($"Starting initial event poll for shard {e.Client.ShardId}");
            await PollAndScheduleEvents(e.Client);
            logger.LogInformation($"Initial event poll completed for shard {e.Client.ShardId}");
            logger.LogInformation($"Starting poll timer for shard {e.Client.ShardId}");
            StartEventPollTimer(e.Client);
            logger.LogInformation($"Poll timer started for shard {e.Client.ShardId}");
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
                var logger = _serviceProvider.GetService<ILogger<Program>>();
                var errorId = Guid.NewGuid();
                logger.LogError($"{errorId}: {e.Exception.Message}\n{e.Exception.StackTrace}");

                var ravenClient = _serviceProvider.GetService<IRavenClient>();
                string sentryEventId = string.Empty;
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

        private async Task<int> ResolvePrefix(DiscordMessage msg)
        {
            var cache = _serviceProvider.GetRequiredService<IMemoryCache>();
            if (!cache.TryGetValue($"prefix:{msg.Channel.GuildId}", out string prefix))
            {
                var calendarService = _serviceProvider.GetService<ICalendarService>();
                prefix = await calendarService.GetCalendarPrefixAsync(msg.Channel.GuildId);
                var defaultPrefix = _configuration.GetSection("Bot")
                    .GetSection("Prefixes")
                    .Get<string[]>()[0];
                
                if (string.IsNullOrEmpty(prefix))
                {
                    prefix = defaultPrefix;
                }

                // Store prefix in cache
                cache.Set($"prefix:{msg.Channel.GuildId}", prefix, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(3)));
            }

            if (!msg.Content.StartsWith(prefix))
            {
                return -1;
            }

            return prefix.Length;
        }

        private void StartEventPollTimer(DiscordClient client)
        {
            Timer t = new Timer(60 * 60 * 1000)
            {
                AutoReset = true
            };
            t.Elapsed += new ElapsedEventHandler(async (sender, e) => await PollAndScheduleEvents(client));
            t.Start();
        }

        private async Task PollAndScheduleEvents(DiscordClient client)
        {
            var logger = _serviceProvider.GetService<ILogger<Program>>();
            var eventScheduler = _serviceProvider.GetService<IEventScheduler>();
            logger.LogInformation($"Polling for events for shard {client.ShardId}");
            await eventScheduler.PollAndScheduleEvents(client);
        }

        private void OnLogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            var logger = _serviceProvider.GetService<ILogger<Program>>();
            switch (e.Level)
            {
                case DSharpPlus.LogLevel.Critical:
                    logger.LogCritical($"[{e.Application}] {e.Message}");
                    break;
                case DSharpPlus.LogLevel.Debug:
                    logger.LogDebug($"[{e.Application}] {e.Message}");
                    break;
                case DSharpPlus.LogLevel.Error:
                    logger.LogError($"[{e.Application}] {e.Message}");
                    break;
                case DSharpPlus.LogLevel.Info:
                    logger.LogInformation($"[{e.Application}] {e.Message}");
                    break;
                case DSharpPlus.LogLevel.Warning:
                    logger.LogWarning($"[{e.Application}] {e.Message}");
                    break;
            }
        }
    }
}