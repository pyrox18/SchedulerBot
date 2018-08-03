using System;
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
using SchedulerBot.Client.Commands;
using SchedulerBot.Data;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;
using System.Collections.Generic;
using SchedulerBot.Client.Scheduler;
using NLog.Extensions.Logging;

namespace SchedulerBot.Client
{
    class Program
    {
        private IConfigurationRoot Configuration { get; set; }
        private DiscordClient Client { get; set; }
        private IServiceProvider ServiceProvider { get; set; }

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
            Configuration = Configure(environment);
            
            Console.WriteLine("Configuring services...");
            ConfigureServices();

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
            Client.DebugLogger.LogMessageReceived += OnLogMessageReceived;

            logger.LogInformation("Initialising command module");
            var commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                PrefixResolver = ResolvePrefix,
                Services = ServiceProvider
            });

            logger.LogInformation("Registering commands");
            commands.RegisterCommands<AdminCommands>();
            commands.RegisterCommands<EventCommands>();
            commands.RegisterCommands<InitializerCommands>();
            commands.RegisterCommands<MiscCommands>();
            commands.RegisterCommands<PermissionsCommands>();
            commands.RegisterCommands<SettingsCommands>();

            // Register event handlers
            logger.LogInformation("Registering event handlers");
            Client.GuildCreated += OnGuildCreate;
            Client.GuildDeleted += OnGuildDelete;
            Client.GuildMemberRemoved += OnGuildMemberRemove;
            Client.GuildRoleDeleted += OnGuildRoleDelete;

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

        private IConfigurationRoot Configure(string environment)
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

            return builder.Build();
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
