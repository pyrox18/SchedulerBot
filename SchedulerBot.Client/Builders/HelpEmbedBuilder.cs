using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;

namespace SchedulerBot.Client.Builders
{
    public class HelpEmbedBuilder
    {
        private DiscordEmbedBuilder _embed;

        public HelpEmbedBuilder()
        {
            _embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor(211, 255, 219)
            };
            _embed.WithAuthor("SchedulerBot", iconUrl: "https://cdn.discordapp.com/avatars/339019867325726722/e5fca7dbae7156e05c013766fa498fe1.png");
            _embed.WithFooter("Syntax: <> = required field, [] = optional field, | = select one. You're not supposed to type these characters in the actual command.");
        }

        public HelpEmbedBuilder WithCommandString(string commandString)
        {
            _embed.Title = $"Help: {commandString}";
            return this;
        }

        public HelpEmbedBuilder WithDescription(string description)
        {
            _embed.Description = description;
            return this;
        }

        public HelpEmbedBuilder WithUsage(string usage)
        {
            _embed.AddField("Usage", $"`{usage}`");
            return this;
        }

        public HelpEmbedBuilder WithUsage(string usage, Dictionary<string, string> options)
        {
            var sb = new StringBuilder().AppendLine($"`{usage}`");
            foreach (var option in options)
            {
                sb.AppendLine($"- `{option.Key}`: {option.Value}");
            }

            _embed.AddField("Usage", sb.ToString());
            return this;
        }

        public HelpEmbedBuilder WithOptions(Dictionary<string, string> options)
        {
            var sb = new StringBuilder();
            foreach (var option in options)
            {
                sb.AppendLine($"- `{option.Key}`: {option.Value}");
            }
            _embed.AddField("Options", sb.ToString());
            return this;
        }

        public HelpEmbedBuilder WithExamples(IEnumerable<string> examples)
        {
            var sb = new StringBuilder();
            foreach (var example in examples)
            {
                sb.AppendLine($"`{example}`");
            }

            _embed.AddField("Examples", sb.ToString());
            return this;
        }

        public HelpEmbedBuilder WithAvailableSubcommands(IEnumerable<string> subcommands)
        {
            var subcommandString = string.Join(", ", subcommands);
            _embed.AddField("Available Subcommands", subcommandString);
            return this;
        }

        public DiscordEmbed Build()
        {
            return _embed.Build();
        }
    }
}
