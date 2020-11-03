using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Caching.Memory;
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
using SharpRaven;
using SharpRaven.Data;
using SchedulerBot.Client.Commands;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Scheduler;
using SchedulerBot.Data;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;
using DSharpPlus.CommandsNext.Exceptions;
using SchedulerBot.Client.Services;
using DSharpPlus.Interactivity.Extensions;

namespace SchedulerBot.Client
{
    class Program
    {
        private IConfigurationRoot Configuration { get; set; }
        private DiscordShardedClient Client { get; set; }
        private IServiceProvider ServiceProvider { get; set; }

        static void Main(string[] args = null)
        {
            new Program().MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task MainAsync(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Reading configuration file...");
            Configure();
            
            Console.WriteLine("Configuring services...");
            ConfigureServices();

            var logger = ServiceProvider.GetService<ILogger<Program>>();
            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

            // Bot
            logger.LogInformation($"SchedulerBot v{version}");

            // Apply deletes and repeats to events that have ended
            var eventService = ServiceProvider.GetService<IEventService>();
            logger.LogInformation("Deleting and repeating past events");
            await eventService.ApplyDeleteAndRepeatPastEventsAsync();

            logger.LogInformation("Initialising client");
            var config = new DiscordConfiguration
            {
                Token = Configuration.GetSection("Bot").GetValue<string>("Token"),
                TokenType = TokenType.Bot
            };
            var logLevel = Configuration.GetSection("Logging").GetSection("LogLevel").GetValue<string>("Default");
            config.MinimumLogLevel = logLevel == "Information" ? LogLevel.Information : Enum.Parse<LogLevel>(logLevel);
            Client = new DiscordShardedClient(config);
            await Client.UseInteractivityAsync(new InteractivityConfiguration());

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

        private void Configure()
        {
            var builder = new ConfigurationBuilder();

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.IsNullOrWhiteSpace(environment))
            {
                throw new ArgumentNullException("Environment not found in ASPNETCORE_ENVIRONMENT");
            }

            Console.WriteLine($"Environment: {environment}");
            if (environment == "Development")
            {
                builder.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), string.Format("..{0}..{0}..{0}", Path.DirectorySeparatorChar)));
            }
            else
            {
                builder.SetBasePath(Directory.GetCurrentDirectory());
            }
            builder.AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();
            var connectionString = Configuration.GetConnectionString("SchedulerBotContext");

            var loggerFactory = new LoggerFactory();
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
            services.AddLogging(options =>
            {
                var logLevel = Enum.Parse<Microsoft.Extensions.Logging.LogLevel>(Configuration.GetSection("Logging").GetSection("LogLevel").GetValue<string>("Default"));
                options.SetMinimumLevel(logLevel);
            });

            // Add configuration as a service
            services.AddSingleton(Configuration);

            // Add cache service for caching prefixes
            services.AddMemoryCache();

            // Add Raven client as a service for production environment
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
            {
                var dsn = Configuration.GetSection("Raven").GetValue<string>("DSN");
                var ravenClient = new RavenClient(dsn);
                services.AddSingleton<IRavenClient>(ravenClient);
            }

            // Configure database
            services.AddSingleton(new SchedulerBotContextFactory(connectionString));

            services.AddSingleton<ICalendarService, CalendarService>()
                .AddSingleton<IEventService, EventService>()
                .AddSingleton<IPermissionService, PermissionService>()
                .AddSingleton<IShardedClientInformationService, ShardedClientInformationService>(s => new ShardedClientInformationService(Client));
                
            // Scheduler service
            services.AddSingleton<IEventScheduler, EventScheduler>();

            ServiceProvider = services.BuildServiceProvider();

            // NLog configuration
            loggerFactory.AddNLog(new NLogProviderOptions
            {
                CaptureMessageProperties = true,
                CaptureMessageTemplates = true
            });
            NLog.LogManager.LoadConfiguration("nlog.config");

        }

        private async Task OnGuildCreate(DiscordClient client, GuildCreateEventArgs e)
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

        private async Task OnGuildDelete(DiscordClient client, GuildDeleteEventArgs e)
        {
            var calendarService = ServiceProvider.GetService<ICalendarService>();
            await calendarService.DeleteCalendarAsync(e.Guild.Id);
        }

        private async Task OnGuildMemberRemove(DiscordClient client, GuildMemberRemoveEventArgs e)
        {
            var permissionService = ServiceProvider.GetService<IPermissionService>();
            await permissionService.RemoveUserPermissionsAsync(e.Guild.Id, e.Member.Id);
        }

        private async Task OnGuildRoleDelete(DiscordClient client, GuildRoleDeleteEventArgs e)
        {
            var permissionService = ServiceProvider.GetService<IPermissionService>();
            await permissionService.RemoveRolePermissionsAsync(e.Guild.Id, e.Role.Id);
        }

        private async Task OnClientReady(DiscordClient client, ReadyEventArgs e)
        {
            var logger = ServiceProvider.GetService<ILogger<Program>>();

            // Set status
            logger.LogInformation("Updating status");
            var version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            await client.UpdateStatusAsync(new DiscordActivity(string.Format(Configuration.GetSection("Bot").GetValue<string>("Status"), version)));

            // Start event polling
            logger.LogInformation($"Starting initial event poll for shard {client.ShardId}");
            await PollAndScheduleEvents(client);
            logger.LogInformation($"Initial event poll completed for shard {client.ShardId}");
            logger.LogInformation($"Starting poll timer for shard {client.ShardId}");
            StartEventPollTimer(client);
            logger.LogInformation($"Poll timer started for shard {client.ShardId}");
        }

        private async Task OnCommandError(CommandsNextExtension extension, CommandErrorEventArgs e)
        {
            var exceptionType = e.Exception.GetType();
            if (exceptionType != typeof(CommandNotFoundException) && exceptionType != typeof(ArgumentException) && exceptionType != typeof(UnauthorizedException) && exceptionType != typeof(InvalidOperationException))
            {
                var logger = ServiceProvider.GetService<ILogger<Program>>();
                var errorId = Guid.NewGuid();
                logger.LogError($"{errorId}: {e.Exception.Message}\n{e.Exception.StackTrace}");

                var ravenClient = ServiceProvider.GetService<IRavenClient>();
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
            var cache = ServiceProvider.GetRequiredService<IMemoryCache>();
            if (!cache.TryGetValue($"prefix:{msg.Channel.GuildId}", out string prefix))
            {
                var calendarService = ServiceProvider.GetService<ICalendarService>();
                prefix = await calendarService.GetCalendarPrefixAsync(msg.Channel.GuildId);
                var defaultPrefix = Configuration.GetSection("Bot").GetSection("Prefixes").Get<string[]>()[0];
                
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
            var logger = ServiceProvider.GetService<ILogger<Program>>();
            var eventScheduler = ServiceProvider.GetService<IEventScheduler>();
            logger.LogInformation($"Polling for events for shard {client.ShardId}");
            await eventScheduler.PollAndScheduleEvents(client);
        }
    }
}
