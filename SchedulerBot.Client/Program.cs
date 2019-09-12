using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
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
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var hostBuilder = new HostBuilder()
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

                    var loggerFactory = new LoggerFactory();
                    loggerFactory.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageProperties = true,
                        CaptureMessageTemplates = true
                    });
                    NLog.LogManager.LoadConfiguration("nlog.config");
                    services.AddSingleton<ILoggerFactory>(loggerFactory);
                    services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                    services.AddLogging(options =>
                    {
                        var logLevel = Enum.Parse<Microsoft.Extensions.Logging.LogLevel>(configuration.GetSection("Logging").GetSection("LogLevel").GetValue<string>("Default"));
                        options.SetMinimumLevel(logLevel);
                    });

                    // TODO: Remove
                    // Add configuration as a service
                    //services.AddSingleton(configuration);

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
                .ConfigureBot((hostContext, configBuilder) =>
                {
                    var config = hostContext.Configuration;
                    var token = config.GetSection("Bot").GetValue<string>("Token");
                    var logLevel = config.GetSection("Logging").GetSection("LogLevel").GetValue<string>("Default");

                    configBuilder.WithToken(token);
                    configBuilder.WithLogLevel(logLevel);
                });

            await hostBuilder.RunConsoleAsync();
        }
    }
}
