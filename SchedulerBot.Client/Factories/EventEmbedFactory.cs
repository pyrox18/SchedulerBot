using System;
using System.Globalization;
using System.Text;
using DSharpPlus.Entities;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Client.Factories
{
    public static class EventEmbedFactory
    {
        private static readonly DiscordColor _createEventColour = new DiscordColor(124, 174, 255);
        private static readonly DiscordColor _deleteEventColour = new DiscordColor(255, 43, 43);
        private static readonly DiscordColor _notifyEventColour = new DiscordColor(20, 255, 71);
        private static readonly DiscordColor _remindEventColour = new DiscordColor(0, 216, 255);
        private static readonly DiscordColor _updateEventColour = new DiscordColor(255, 248, 73);
        private static readonly DiscordColor _viewEventColour = new DiscordColor(48, 229, 202);

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
            embed.AddField("Start Date", evt.StartTimestamp.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture), true);
            embed.AddField("End Date", evt.EndTimestamp.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture), true);

            if (evt.ReminderTimestamp != null)
            {
                embed.AddField("Reminder", ((DateTimeOffset)evt.ReminderTimestamp).ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture), true);
            }
            else
            {
                embed.AddField("Reminder", "N/A", true);
            }

            string repeatString;
            switch (evt.Repeat)
            {
                case RepeatType.None:
                    repeatString = "N/A";
                    break;
                case RepeatType.MonthlyWeekday:
                    repeatString = "Monthly by weekday";
                    break;
                default:
                    repeatString = evt.Repeat.ToString();
                    break;
            }
            embed.AddField("Repeat", repeatString);

            StringBuilder mentionStringBuilder = new StringBuilder();
            if (evt.Mentions != null)
            {
                foreach (var mention in evt.Mentions)
                {
                    switch (mention.Type)
                    {
                        case MentionType.Role:
                            mentionStringBuilder.Append($"{mention.TargetId.AsRoleMention()} ");
                            break;
                        case MentionType.User:
                            mentionStringBuilder.Append($"{mention.TargetId.AsUserMention()} ");
                            break;
                        case MentionType.Everyone:
                            mentionStringBuilder.Append("@everyone");
                            break;
                        case MentionType.RSVP:
                            mentionStringBuilder.Append("All RSVP'd users ");
                            break;
                    }
                }
            }
            embed.AddField("Mentions", evt.Mentions != null && evt.Mentions.Count > 0 ? mentionStringBuilder.ToString() : "N/A");

            StringBuilder rsvpStringBuilder = new StringBuilder();
            if (evt.RSVPs != null)
            {
                foreach (var rsvp in evt.RSVPs)
                {
                    rsvpStringBuilder.Append($"{rsvp.UserId.AsUserMention()} ");
                }
            }
            embed.AddField("RSVPs", evt.RSVPs != null && evt.RSVPs.Count > 0 ? rsvpStringBuilder.ToString() : "N/A");

            return embed;
        }

        private static DiscordEmbedBuilder GetBaseEmbed(EventViewModel evt)
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
            embed.AddField("Start Date", evt.StartTimestamp.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture), true);
            embed.AddField("End Date", evt.EndTimestamp.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture), true);

            if (evt.ReminderTimestamp != null)
            {
                embed.AddField("Reminder", ((DateTimeOffset)evt.ReminderTimestamp).ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture), true);
            }
            else
            {
                embed.AddField("Reminder", "N/A", true);
            }

            string repeatString;
            switch (evt.Repeat)
            {
                case RepeatType.None:
                    repeatString = "N/A";
                    break;
                case RepeatType.MonthlyWeekday:
                    repeatString = "Monthly by weekday";
                    break;
                default:
                    repeatString = evt.Repeat.ToString();
                    break;
            }
            embed.AddField("Repeat", repeatString);

            StringBuilder mentionStringBuilder = new StringBuilder();
            if (evt.Mentions != null)
            {
                foreach (var mention in evt.Mentions)
                {
                    switch (mention.Type)
                    {
                        case MentionType.Role:
                            mentionStringBuilder.Append($"{mention.TargetId.AsRoleMention()} ");
                            break;
                        case MentionType.User:
                            mentionStringBuilder.Append($"{mention.TargetId.AsUserMention()} ");
                            break;
                        case MentionType.Everyone:
                            mentionStringBuilder.Append("@everyone");
                            break;
                        case MentionType.RSVP:
                            mentionStringBuilder.Append("All RSVP'd users ");
                            break;
                    }
                }
            }
            embed.AddField("Mentions", evt.Mentions != null && evt.Mentions.Count > 0 ? mentionStringBuilder.ToString() : "N/A");

            StringBuilder rsvpStringBuilder = new StringBuilder();
            if (evt.RSVPs != null)
            {
                foreach (var rsvp in evt.RSVPs)
                {
                    rsvpStringBuilder.Append($"{rsvp.UserId.AsUserMention()} ");
                }
            }
            embed.AddField("RSVPs", evt.RSVPs != null && evt.RSVPs.Count > 0 ? rsvpStringBuilder.ToString() : "N/A");

            return embed;
        }

        public static DiscordEmbed GetCreateEventEmbed(Event evt)
        {
            var embed = _getBaseEmbed(evt);
            embed.Title = "New Event";
            embed.Color = _createEventColour;
            return embed.Build();
        }

        public static DiscordEmbed GetCreateEventEmbed(EventViewModel evt)
        {
            var embed = GetBaseEmbed(evt);
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

        public static DiscordEmbed GetUpdateEventEmbed(Event evt)
        {
            var embed = _getBaseEmbed(evt);
            embed.Title = "Update Event";
            embed.Color = _updateEventColour;
            return embed.Build();
        }

        public static DiscordEmbed GetNotifyEventEmbed(Event evt)
        {
            var embed = _getBaseEmbed(evt);
            embed.Title = "Event starting now!";
            embed.Color = _notifyEventColour;
            return embed.Build();
        }

        public static DiscordEmbed GetViewEventEmbed(Event evt)
        {
            var embed = _getBaseEmbed(evt);
            embed.Title = "View Event";
            embed.Color = _viewEventColour;
            return embed.Build();
        }

        public static DiscordEmbed GetViewEventEmbed(EventViewModel evt)
        {
            var embed = GetBaseEmbed(evt);
            embed.Title = "View Event";
            embed.Color = _viewEventColour;
            return embed.Build();
        }

        public static DiscordEmbed GetRemindEventEmbed(Event evt)
        {
            var embed = _getBaseEmbed(evt);
            embed.Title = "Event Reminder";
            embed.Color = _remindEventColour;
            return embed.Build();
        }
    }
}
