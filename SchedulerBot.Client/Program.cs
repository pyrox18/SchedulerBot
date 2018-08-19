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

namespace SchedulerBot.Client
{
    class Program
    {
        private IConfigurationRoot Configuration { get; set; }
        private DiscordClient Client { get; set; }
        private InteractivityExtension Interactivity { get; set; }
        private IServiceProvider ServiceProvider { get; set; }
        private RavenClient RavenClient { get; set; }

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

            // Initialise Raven service if production environment
            if (environment == "Production")
            {
                var dsn = Configuration.GetSection("Raven").GetValue<string>("DSN");
                RavenClient = new RavenClient(dsn);
            }

            var logger = ServiceProvider.GetService<ILogger<Program>>();

            // Bot
            logger.LogInformation($"SchedulerBot v{Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
            logger.LogInformation("Initialising client");
            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = Configuration.GetSection("Bot").GetValue<string>("Token"),
                TokenType = TokenType.Bot,
                LogLevel = DSharpPlus.LogLevel.Debug,
            });
            Interactivity = Client.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehavior = TimeoutBehaviour.DeleteReactions
            });
            Client.DebugLogger.LogMessageReceived += OnLogMessageReceived;

            logger.LogInformation("Initialising command module");
            var commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                PrefixResolver = ResolvePrefix,
                Services = ServiceProvider,
                EnableDefaultHelp = false
            });

            logger.LogInformation("Registering commands");
            commands.RegisterCommands<AdminCommands>();
            commands.RegisterCommands<EventCommands>();
            commands.RegisterCommands<InitializerCommands>();
            commands.RegisterCommands<MiscCommands>();
            commands.RegisterCommands<PermissionsCommands>();
            commands.RegisterCommands<SettingsCommands>();
            commands.RegisterCommands<HelpCommands>();

            // Register event handlers
            logger.LogInformation("Registering event handlers");
            Client.GuildCreated += OnGuildCreate;
            Client.GuildDeleted += OnGuildDelete;
            Client.GuildMemberRemoved += OnGuildMemberRemove;
            Client.GuildRoleDeleted += OnGuildRoleDelete;
            commands.CommandErrored += OnCommandError;

            // Start event scheduler
            var scheduler = ServiceProvider.GetService<IEventScheduler>();
            logger.LogInformation("Starting event scheduler");
            await scheduler.Start();

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

            logger.LogInformation("Connecting bot");
            Console.WriteLine("Connecting...");
            await Client.ConnectAsync();
            logger.LogInformation("Bot connected");
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

        private async Task OnCommandError(CommandErrorEventArgs e)
        {
            if (e.Exception.GetType() != typeof(ArgumentException))
            {
                var logger = ServiceProvider.GetService<ILogger<Program>>();
                var errorId = Guid.NewGuid();
                logger.LogError($"{errorId}: {e.Exception.Message}\n{e.Exception.StackTrace}");

                if (RavenClient != null)
                {
                    e.Exception.Data.Add("ErrorEventId", errorId.ToString());
                    e.Exception.Data.Add("Message", e.Context.Message);
                    e.Exception.Data.Add("Command", e.Command.QualifiedName);
                    e.Exception.Data.Add("User", e.Context.Member.GetUsernameAndDiscriminator());
                    e.Exception.Data.Add("UserId", e.Context.Member.Id);

                    await RavenClient.CaptureAsync(new SentryEvent(e.Exception));
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
            return await calendarService.ResolveCalendarPrefixAsync(msg.Channel.GuildId, msg.Content);
        }

        private async void PollerTimerElapsed(object sender, ElapsedEventArgs e)
        {
            await PollAndScheduleEvents();
        }

        private async Task PollAndScheduleEvents()
        {
            var eventScheduler = ServiceProvider.GetService<IEventScheduler>();
            await eventScheduler.PollAndScheduleEvents(Client);
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
