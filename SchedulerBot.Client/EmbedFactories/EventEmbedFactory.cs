using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DSharpPlus.Entities;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Client.EmbedFactories
{
    public static class EventEmbedFactory
    {
        private static readonly DiscordColor _createEventColour = new DiscordColor(124, 174, 255);
        private static readonly DiscordColor _deleteEventColour = new DiscordColor(255, 43, 43);
        private static readonly DiscordColor _notifyEventColour = new DiscordColor(20, 255, 71);
        private static readonly DiscordColor _updateEventColour = new DiscordColor(255, 248, 73);

        private static DiscordEmbedBuilder _getBaseEmbed(Event evt)
        {
            var embed = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = "SchedulerBot",
                    IconUrl = "https://cdn.discordapp.com/avatars/339019867325726722/e5fca7dbae7156e05c013766fa498fe1.png"
                },
            };
            embed.AddField("Event Name", evt.Name);
            embed.AddField("Description", string.IsNullOrEmpty(evt.Description) ? "N/A" : evt.Description);
            embed.AddField("Start Date", evt.StartTimestamp.ToString("ddd d/M/yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture), true);
            embed.AddField("End Date", evt.EndTimestamp.ToString("ddd d/M/yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture), true);
            embed.AddField("Repeat", evt.Repeat == RepeatType.None ? "N/A" : evt.Repeat.ToString());

            return embed;
        }

        public static DiscordEmbed GetCreateEventEmbed(Event evt)
        {
            var embed = _getBaseEmbed(evt);
            embed.Title = "New Event";
            embed.Color = _createEventColour;
            return embed.Build();
        }

        public static DiscordEmbed GetDeleteEventEmbed(Event evt)
        {
            var embed = _getBaseEmbed(evt);
            embed.Title = "Delete Event";
            embed.Color = _deleteEventColour;
            return embed.Build();
        }
    }
}
