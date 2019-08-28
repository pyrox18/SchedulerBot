using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DSharpPlus.Interactivity;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Client.Factories
{
    public static class EventListPageFactory
    {
        public static List<Page> GetEventListPages(IEnumerable<Event> events)
        {
            StringBuilder sb = new StringBuilder();
            List<Page> pages = new List<Page>();
            ushort i = 0;
            bool activeEventHeaderWritten = false;
            bool upcomingEventHeaderWritten = false;
            DateTimeOffset now = DateTimeOffset.Now;

            foreach (var evt in events)
            {
                if (i % 10 == 0)
                {
                    if (i != 0)
                    {
                        sb.AppendLine("```");
                        sb.AppendLine("Run `event list <event number>` to view details for a certain event.");
                        pages.Add(new Page(sb.ToString()));
                        activeEventHeaderWritten = false;
                        upcomingEventHeaderWritten = false;
                    }

                    sb = new StringBuilder();
                    sb.AppendLine("```css");
                }

                if (evt.StartTimestamp <= now && !activeEventHeaderWritten)
                {
                    sb.AppendLine("[Active Events]");
                    sb.AppendLine();
                    activeEventHeaderWritten = true;
                }
                else if (evt.StartTimestamp > now && !upcomingEventHeaderWritten)
                {
                    sb.AppendLine();
                    sb.AppendLine("[Upcoming Events]");
                    sb.AppendLine();
                    upcomingEventHeaderWritten = true;
                }
                sb.AppendLine($"{i + 1}: {evt.Name} /* {evt.StartTimestamp.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture)} to {evt.EndTimestamp.ToString("ddd d MMM yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture)} */");

                i++;
            }

            // Push last page to list
            sb.AppendLine("```");
            sb.AppendLine("Run `event list <event number>` to view details for a certain event.");
            pages.Add(new Page(sb.ToString()));

            return pages;
        }
    }
}
