using System;
using DSharpPlus;

namespace SchedulerBot.Client.Configuration
{
    public class DiscordConfigurationBuilder
    {
        private readonly DiscordConfiguration _configuration;

        public DiscordConfigurationBuilder()
        {
            _configuration = new DiscordConfiguration();
            _configuration.TokenType = TokenType.Bot;
        }

        public DiscordConfigurationBuilder WithToken(string token)
        {
            _configuration.Token = token;
            return this;
        }

        public DiscordConfigurationBuilder WithLogLevel(string logLevel)
        {
            _configuration.LogLevel = logLevel == "Information"
                ? LogLevel.Info : Enum.Parse<LogLevel>(logLevel);
            return this;
        }

        public DiscordConfiguration Build()
        {
            return _configuration;
        }
    }
}