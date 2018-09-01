using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using NLog.Extensions.Logging;
using RedLockNet;
using SharpRaven;
using SharpRaven.Data;
using SchedulerBot.Client.Commands;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Scheduler;
using SchedulerBot.Data;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;
using RedLockNet.SERedis.Configuration;
using System.Net;
using RedLockNet.SERedis;

namespace SchedulerBot.Client
{
    class Program
    {
        private IConfigurationRoot Configuration { get; set; }
        private DiscordShardedClient Client { get; set; }
        private IServiceProvider ServiceProvider { get; set; }
        private bool _initialPollDone = false;

        static void Main(string[] args = null)
        {
            new Program().MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task MainAsync(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (String.IsNullOrWhiteSpace(environment))
            {
                throw new ArgumentNullException("Environment not found in ASPNETCORE_ENVIRONMENT");
            }

            Console.WriteLine($"Environment: {environment}");

            Console.WriteLine("Reading configuration file...");
            Configure(environment);
            
            Console.WriteLine("Configuring services...");
            ConfigureServices();

            var logger = ServiceProvider.GetService<ILogger<Program>>();
            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

            // Bot
            logger.LogInformation($"SchedulerBot v{version}");
            logger.LogInformation("Initialising client");
            Client = new DiscordShardedClient(new DiscordConfiguration
            {
                Token = Configuration.GetSection("Bot").GetValue<string>("Token"),
                TokenType = TokenType.Bot,
                LogLevel = DSharpPlus.LogLevel.Debug,
            });
            await Client.UseInteractivityAsync(new InteractivityConfiguration
            {
                PaginationBehavior = TimeoutBehaviour.DeleteReactions
            });
            Client.DebugLogger.LogMessageReceived += OnLogMessageReceived;

            logger.LogInformation("Initialising command module");
            var commandsNextExtensions = await Client.UseCommandsNextAsync(new CommandsNextConfiguration
            {
                PrefixResolver = ResolvePrefix,
                Services = ServiceProvider,
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
            Client.GuildCreated += OnGuildCreate;
            Client.GuildDeleted += OnGuildDelete;
            Client.GuildMemberRemoved += OnGuildMemberRemove;
            Client.GuildRoleDeleted += OnGuildRoleDelete;
            Client.Ready += OnClientReady;

            // Start event scheduler
            var scheduler = ServiceProvider.GetService<IEventScheduler>();
            logger.LogInformation("Starting event scheduler");
            await scheduler.Start();


            logger.LogInformation("Connecting all shards...");
            await Client.StartAsync();
            logger.LogInformation("All shards connected");

            await Task.Delay(-1);

            await scheduler.Shutdown();
            NLog.LogManager.Shutdown();
        }

        private void Configure(string environment)
        {
            var builder = new ConfigurationBuilder();
            if (environment == "Development")
            {
                builder.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), string.Format("..{0}..{0}..{0}", Path.DirectorySeparatorChar)));
            }
            else
            {
                builder.SetBasePath(Directory.GetCurrentDirectory());
            }
            builder.AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json");

            Configuration = builder.Build();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();
            var connectionString = Configuration.GetConnectionString("SchedulerBotContext");

            services.AddSingleton<ILoggerFactory, LoggerFactory>();
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(options =>
            {
                var logLevel = Enum.Parse<Microsoft.Extensions.Logging.LogLevel>(Configuration.GetSection("Logging").GetSection("LogLevel").GetValue<string>("Default"));
                options.SetMinimumLevel(logLevel);
            });

            // Add configuration as a service
            services.AddSingleton(Configuration);

            // Add Raven client as a service for production environment
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                var dsn = Configuration.GetSection("Raven").GetValue<string>("DSN");
                var ravenClient = new RavenClient(dsn);
                services.AddSingleton<IRavenClient, RavenClient>();
            }

            // Redis lock factory configuration
            var endpoints = new List<RedLockEndPoint>
            {
                new DnsEndPoint(Configuration.GetConnectionString("Redis"), 6379)
            };
            var redlockFactory = RedLockFactory.Create(endpoints);
            services.AddSingleton<IDistributedLockFactory>(redlockFactory);

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<SchedulerBotContext>(options =>
                {
                    options.UseNpgsql(connectionString);
                });

            services.AddSingleton<ICalendarService, CalendarService>()
                .AddSingleton<IEventService, EventService>()
                .AddSingleton<IPermissionService, PermissionService>();

            // Scheduler service
            services.AddSingleton<IEventScheduler, EventScheduler>();

            ServiceProvider = services.BuildServiceProvider();

            var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

            // NLog configuration
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
                Prefix = Configuration.GetSection("Bot").GetSection("Prefixes").Get<string[]>()[0]
            };

            var calendarService = ServiceProvider.GetService<ICalendarService>();
            await calendarService.CreateCalendarAsync(calendar);
        }

        private async Task OnGuildDelete(GuildDeleteEventArgs e)
        {
            var calendarService = ServiceProvider.GetService<ICalendarService>();
            await calendarService.DeleteCalendarAsync(e.Guild.Id);
        }

        private async Task OnGuildMemberRemove(GuildMemberRemoveEventArgs e)
        {
            var permissionService = ServiceProvider.GetService<IPermissionService>();
            await permissionService.RemoveUserPermissionsAsync(e.Guild.Id, e.Member.Id);
        }

        private async Task OnGuildRoleDelete(GuildRoleDeleteEventArgs e)
        {
            var permissionService = ServiceProvider.GetService<IPermissionService>();
            await permissionService.RemoveRolePermissionsAsync(e.Guild.Id, e.Role.Id);
        }

        private async Task OnClientReady(ReadyEventArgs e)
        {
            // Set status
            var logger = ServiceProvider.GetService<ILogger<Program>>();
            logger.LogInformation("Updating status");
            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            await e.Client.UpdateStatusAsync(new DiscordActivity(string.Format(Configuration.GetSection("Bot").GetValue<string>("Status"), version)));

            // Start event polling
            logger.LogInformation("Starting initial event poll");
            await PollAndScheduleEvents();
            logger.LogInformation("Initial event poll completed");
            logger.LogInformation("Starting poll timer");
            Timer t = new Timer(60 * 60 * 1000)
            {
                AutoReset = true
            };
            t.Elapsed += new ElapsedEventHandler(PollerTimerElapsed);
            t.Start();
            logger.LogInformation("Poll timer started");
        }

        private async Task OnCommandError(CommandErrorEventArgs e)
        {
            var exceptionType = e.Exception.GetType();
            if (exceptionType != typeof(ArgumentException) && exceptionType != typeof(UnauthorizedException))
            {
                var logger = ServiceProvider.GetService<ILogger<Program>>();
                var errorId = Guid.NewGuid();
                logger.LogError($"{errorId}: {e.Exception.Message}\n{e.Exception.StackTrace}");

                var ravenClient = ServiceProvider.GetService<IRavenClient>();
                if (ravenClient != null)
                {
                    e.Exception.Data.Add("ErrorEventId", errorId.ToString());
                    e.Exception.Data.Add("Message", e.Context.Message);
                    e.Exception.Data.Add("Command", e.Command.QualifiedName);
                    e.Exception.Data.Add("User", e.Context.Member.GetUsernameAndDiscriminator());
                    e.Exception.Data.Add("UserId", e.Context.Member.Id);
                    e.Exception.Data.Add("ShardId", e.Context.Client.ShardId);

                    await ravenClient.CaptureAsync(new SentryEvent(e.Exception));
                }

                var sb = new StringBuilder();
                sb.AppendLine("An error has occurred. Please report this in the support server using the `support` command.");
                sb.AppendLine($"Error event ID: {errorId}");
                sb.AppendLine("```");
                sb.AppendLine($"{e.Exception.Message}");
                sb.AppendLine("```");
                await e.Context.RespondAsync(sb.ToString());
            }
        }

        private async Task<int> ResolvePrefix(DiscordMessage msg)
        {
            var calendarService = ServiceProvider.GetService<ICalendarService>();
            var prefix = await calendarService.GetCalendarPrefixAsync(msg.Channel.GuildId);
            var defaultPrefix = Configuration.GetSection("Bot").GetSection("Prefixes").Get<string[]>()[0];

            if (string.IsNullOrEmpty(prefix) && msg.Content.StartsWith(defaultPrefix))
            {
                return defaultPrefix.Length;
            }

            if (string.IsNullOrEmpty(prefix) || !msg.Content.StartsWith(prefix))
            {
                return -1;
            }

            return prefix.Length;
        }

        private async void PollerTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await PollAndScheduleEvents();
        }

        private async Task PollAndScheduleEvents()
        {
            if (!_initialPollDone)
            {
                _initialPollDone = true;
                var eventScheduler = ServiceProvider.GetService<IEventScheduler>();
                await eventScheduler.PollAndScheduleEvents(Client);
            }
        }

        private void OnLogMessageReceived(object sender, DebugLogMessageEventArgs e)
        {
            var logger = ServiceProvider.GetService<ILogger<Program>>();
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
