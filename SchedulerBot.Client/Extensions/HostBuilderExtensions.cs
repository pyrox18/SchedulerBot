using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchedulerBot.Client.Configuration;
using SchedulerBot.Client.Services;
using System;

namespace SchedulerBot.Client.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureBot(this IHostBuilder builder, Action<HostBuilderContext, DiscordConfigurationBuilder> config)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));

            builder.ConfigureServices((hostContext, services) =>
            {
                var configBuilder = new DiscordConfigurationBuilder();
                config(hostContext, configBuilder);
                var botConfig = configBuilder.Build();
                var client = new DiscordShardedClient(botConfig);

                services.AddSingleton(client);
                services.AddSingleton<IShardedClientInformationService, ShardedClientInformationService>();
                services.AddHostedService<Bot>();
            });

            return builder;
        }
    }
}
