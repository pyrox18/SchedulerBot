using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using SchedulerBot.Application.Calendars.Commands.InitialiseCalendar;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Client.Configuration;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Parsers;
using SchedulerBot.Client.Scheduler;
using SchedulerBot.Client.Scheduler.Jobs;
using SchedulerBot.Data;
using SchedulerBot.Data.Services;
using SchedulerBot.Infrastructure;
using SchedulerBot.Persistence;
using SchedulerBot.Persistence.Repositories;
using Serilog;
using SharpRaven;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SchedulerBot.Client
{
    class Program
    {
        public static IConfiguration Configuration { get; }

        static Program()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        static async Task<int> Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            try
            {
                var hostBuilder = CreateHostBuilder(args);

                await hostBuilder.RunConsoleAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal("Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                    configHost.AddEnvironmentVariables();
                    configHost.AddEnvironmentVariables(prefix: "ASPNETCORE_");
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    var environment = hostContext.HostingEnvironment.EnvironmentName;
                    Console.WriteLine($"Environment: {environment}");

                    configApp.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

                    configApp.AddJsonFile("appsettings.json");
                    configApp.AddJsonFile($"appsettings.{environment}.json", true);

                    configApp.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    var connectionString = configuration.GetConnectionString("SchedulerBotContext");
                    var connectionString2 = configuration.GetConnectionString("SchedulerBot");

                    // Add configuration options
                    services.Configure<BotConfiguration>(configuration.GetSection("Bot"));

                    // Add cache service for caching prefixes
                    services.AddMemoryCache();

                    // Add Raven client as a service for production environment
                    if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
                    {
                        var dsn = configuration.GetSection("Raven").GetValue<string>("DSN");
                        var ravenClient = new RavenClient(dsn);
                        services.AddSingleton<IRavenClient>(ravenClient);
                    }

                    // Configure database
                    services.AddSingleton(new SchedulerBotContextFactory(connectionString));
                    services.AddDbContextPool<SchedulerBotDbContext>(options =>
                    {
                        options.UseNpgsql(connectionString2);
                    }, 5); // TODO: Control pool size from config

                    services.AddSingleton<ICalendarService, CalendarService>()
                        .AddSingleton<IEventService, EventService>()
                        .AddSingleton<IPermissionService, PermissionService>();

                    // Repositories
                    services.AddScoped<ICalendarRepository, CalendarRepository>();
                    services.AddScoped<IEventRepository, EventRepository>();
                    services.AddScoped<IPermissionRepository, PermissionRepository>();

                    // Other services
                    services.AddScoped<IEventParser, EventParser>();
                    services.AddScoped<IDateTimeOffset, MachineDateTimeOffset>();

                    // MediatR
                    services.AddMediatR(typeof(InitialiseCalendarCommand).Assembly);

                    // Scheduler service
                    services.AddSingleton<IJobFactory, JobFactory>();
                    services.AddSingleton<ISchedulerFactory>(new StdSchedulerFactory(new NameValueCollection
                    {
                        // TODO: Move this to configuration
                        { "quartz.scheduler.instanceName", "SchedulerBotScheduler" },
                        { "quartz.jobStore.type", "Quartz.Simpl.RAMJobStore, Quartz" },
                        { "quartz.threadPool.threadCount", "3" }
                    }));
                    services.AddScoped<EventDeleteJob>();
                    services.AddScoped<EventNotifyJob>();
                    services.AddScoped<EventPollingJob>();
                    services.AddScoped<EventReminderJob>();
                    services.AddScoped<EventRepeatJob>();
                    services.AddSingleton<QuartzJobRunner>();
                    services.AddSingleton<IEventScheduler, QuartzEventScheduler>();
                    services.AddHostedService<QuartzHostedService>();
                })
                .UseSerilog()
                .ConfigureBot((hostContext, configBuilder) =>
                {
                    var config = hostContext.Configuration;
                    var token = config.GetSection("Bot").GetValue<string>("Token");
                    var logLevel = config.GetSection("Serilog").GetSection("MinimumLevel").GetValue<string>("Default");

                    configBuilder.WithToken(token);
                    configBuilder.WithLogLevel(logLevel);
                });
        }
    }
}
