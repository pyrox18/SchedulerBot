using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using SchedulerBot.Client.Commands;
using SchedulerBot.Data;
using System.Collections.Generic;

namespace SchedulerBot.Client
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static DiscordClient Client { get; set; }

        static void Main(string[] args = null)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (String.IsNullOrWhiteSpace(environment))
            {
                throw new ArgumentNullException("Environment not found in ASPNETCORE_ENVIRONMENT");
            }

            Console.WriteLine($"Environment: {environment}");

            Configuration = Configure(environment);
            
            var serviceProvider = ConfigureServices(new ServiceCollection());

            // Bot

            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = Configuration.GetSection("Bot").GetValue<string>("Token"),
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = DSharpPlus.LogLevel.Debug,
            });

            var commands = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = Configuration.GetSection("Bot").GetSection("Prefixes").Get<string[]>()
            });

            commands.RegisterCommands<AdminCommands>();
            commands.RegisterCommands<EventCommands>();
            commands.RegisterCommands<InitializerCommands>();
            commands.RegisterCommands<MiscCommands>();
            commands.RegisterCommands<PermissionsCommands>();
            commands.RegisterCommands<SettingsCommands>();

            Console.WriteLine("Connecting...");
            await Client.ConnectAsync();
            Console.WriteLine("Bot connected");
            await Task.Delay(-1);
        }

        static IConfigurationRoot Configure(string environment)
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

        static IServiceProvider ConfigureServices(IServiceCollection services)
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

            return services.BuildServiceProvider();
        }
    }
}
