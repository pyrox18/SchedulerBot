using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using SchedulerBot.Client.Commands;
using SchedulerBot.Data;
using SchedulerBot.Data.Models;
using SchedulerBot.Data.Services;
using System.Collections.Generic;
using DSharpPlus.Entities;
using SchedulerBot.Client.Scheduler;

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
            ServiceProvider = ConfigureServices(new ServiceCollection());

            // Bot
            Console.WriteLine("Initialising client...");
            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = Configuration.GetSection("Bot").GetValue<string>("Token"),
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = DSharpPlus.LogLevel.Debug,
            });

            Console.WriteLine("Initialising commands module...");
            var commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                PrefixResolver = ResolvePrefix,
                Services = ServiceProvider
            });

            Console.WriteLine("Registering commands...");
            commands.RegisterCommands<AdminCommands>();
            commands.RegisterCommands<EventCommands>();
            commands.RegisterCommands<InitializerCommands>();
            commands.RegisterCommands<MiscCommands>();
            commands.RegisterCommands<PermissionsCommands>();
            commands.RegisterCommands<SettingsCommands>();

            // Register event handlers
            Console.WriteLine("Registering event handlers...");
            Client.GuildCreated += OnGuildCreate;
            Client.GuildDeleted += OnGuildDelete;
            Client.GuildMemberRemoved += OnGuildMemberRemove;
            Client.GuildRoleDeleted += OnGuildRoleDelete;

            // Start event scheduler
            var scheduler = ServiceProvider.GetService<IEventScheduler>();
            Console.WriteLine("Starting event scheduler...");
            await scheduler.Start();

            Console.WriteLine("Starting initial event poll...");
            await PollAndScheduleEvents();
            Console.WriteLine("Polling completed.");
            Console.WriteLine("Starting poll timer...");
            Timer t = new Timer(60 * 60 * 1000)
            {
                AutoReset = true
            };
            t.Elapsed += new ElapsedEventHandler(PollerTimerElapsed);
            t.Start();
            Console.WriteLine("Timer started.");

            Console.WriteLine("Connecting...");
            await Client.ConnectAsync();
            Console.WriteLine("Bot connected");
            await Task.Delay(-1);

            await scheduler.Shutdown();
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

        private IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("SchedulerBotContext");

            services.AddLogging(options =>
            {
                options.AddConfiguration(Configuration.GetSection("Logging"));
                options.AddConsole();
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

            return services.BuildServiceProvider();
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
    }
}
