using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using NodaTime;
using NodaTime.Extensions;
using NodaTime.Text;
using SchedulerBot.Client.Exceptions;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Client.Parsers
{
    public class EventParser
    {
        private static readonly LocalDateTimePattern _dateTimePattern = LocalDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm:ss");
        private static readonly LocalDateTimePattern _dateOnlyPattern = LocalDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd");

        public static Event ParseNewEvent(string[] args, string timezone)
        {
            var evt = new Event();

            var bodyString = ParseEventInputBody(args);
            ParseEventTimestamps(bodyString, timezone, evt);
            ParseEventInputFlags(args, evt, timezone);

            return evt;
        }

        public static Event ParseUpdateEvent(Event evt, string[] args, string timezone)
        {
            var bodyString = ParseEventInputBody(args);
            if (!string.IsNullOrEmpty(bodyString))
            {
                ParseEventTimestamps(bodyString, timezone, evt, true);
            }
            ParseEventInputFlags(args, evt, timezone);

            return evt;
        }

        public static Event ParseUpdateEvent(Event evt, string args, string timezone)
        {
            return ParseUpdateEvent(evt, args.Split(' '), timezone);
        }

        private static string ParseEventInputBody(string[] args)
        {
            var body = new List<string>();
            uint i = 0;
            int argsLength = args.Length;

            while (i < argsLength && !args[i].StartsWith("--"))
            {
                body.Add(args[i]);
                i++;
            }

            return string.Join(' ', body.ToArray());
        }

        private static void ParseEventInputFlags(string[] args, Event evt, string timezone)
        {
            uint i = 0;
            int argsLength = args.Length;

            while (i < argsLength)
            {
                if (args[i].StartsWith("--"))
                {
                    var key = args[i].Substring(2);
                    i++;
                    var values = new List<string>();
                    while (i < argsLength && !args[i].StartsWith("--"))
                    {
                        values.Add(args[i]);
                        i++;
                    }

                    switch (key)
                    {
                        case "repeat":
                            if (values.Count == 0) evt.Repeat = RepeatType.None;
                            else
                            {

                                switch (values[0])
                                {
                                    case "d":
                                        evt.Repeat = RepeatType.Daily;
                                        break;
                                    case "w":
                                        evt.Repeat = RepeatType.Weekly;
                                        break;
                                    case "m":
                                        evt.Repeat = RepeatType.Monthly;
                                        break;
                                    case "mw":
                                        evt.Repeat = RepeatType.MonthlyWeekday;
                                        break;
                                    case "n":
                                        evt.Repeat = RepeatType.None;
                                        break;
                                    default:
                                        if (evt.Repeat != RepeatType.Daily && evt.Repeat != RepeatType.Weekly && evt.Repeat != RepeatType.Monthly && evt.Repeat != RepeatType.MonthlyWeekday)
                                        {
                                            evt.Repeat = RepeatType.None;
                                        }
                                        break;
                                }
                            }
                            break;
                        case "desc":
                            if (values.Count > 0)
                            {
                                evt.Description = string.Join(' ', values.ToArray());
                            }
                            break;
                        case "mention":
                            evt.Mentions = new List<EventMention>();
                            foreach (var value in values)
                            {
                                if (value == "@everyone")
                                {
                                    evt.Mentions.Clear();
                                    evt.Mentions.Add(new EventMention
                                    {
                                        Type = MentionType.Everyone
                                    });

                                    break;
                                }
                                else if (value.ToLower() == "rsvp")
                                {
                                    evt.Mentions.Add(new EventMention
                                    {
                                        Type = MentionType.RSVP
                                    });
                                }
                                else if (value.StartsWith("<@") && value.EndsWith(">"))
                                {
                                    var mention = new EventMention();
                                    string mentionIdString;
                                    if (value.StartsWith("<@&"))
                                    {
                                        mention.Type = MentionType.Role;
                                        mentionIdString = value.Substring(3);
                                    }
                                    else
                                    {
                                        mention.Type = MentionType.User;
                                        if (value.StartsWith("<@!"))
                                        {
                                            mentionIdString = value.Substring(3);
                                        }
                                        else
                                        {
                                            mentionIdString = value.Substring(2);
                                        }
                                    }
                                    mentionIdString = mentionIdString.TrimEnd('>');
                                    if (ulong.TryParse(mentionIdString, out ulong targetId))
                                    {
                                        mention.TargetId = targetId;
                                        evt.Mentions.Add(mention);
                                    }
                                }
                            }
                            break;
                        case "remind":
                            var reminderString = string.Join(' ', values);
                            var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timezone);
                            if (tz == null)
                            {
                                throw new InvalidTimeZoneException();
                            }

                            var results = DateTimeRecognizer.RecognizeDateTime(reminderString, Culture.English);
                            if (results.Count > 0 && results.First().TypeName.StartsWith("datetimeV2"))
                            {
                                var first = results.First();
                                var resolutionValues = (IList<Dictionary<string, string>>)first.Resolution["values"];

                                var subType = first.TypeName.Split('.').Last();
                                if (subType == "duration")
                                {
                                    string value = resolutionValues.Select(v => v["value"]).FirstOrDefault();
                                    double seconds = double.Parse(value);
                                    var reminderTimestamp = evt.StartTimestamp.AddSeconds(-seconds);
                                    if (IsFuture(reminderTimestamp))
                                    {
                                        evt.ReminderTimestamp = reminderTimestamp;
                                    }
                                    else
                                    {
                                        throw new DateTimeInPastException();
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    i++;
                }
            }
        }

        private static void ParseEventTimestamps(string bodyString, string timezone, Event evt, bool isUpdate = false)
        {
            string newBodyString = "";
            var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timezone);
            if (tz == null)
            {
                throw new InvalidTimeZoneException();
            }

            var results = DateTimeRecognizer.RecognizeDateTime(bodyString, Culture.English);
            if (results.Count > 0 && results.Any(r => r.TypeName.StartsWith("datetimeV2")))
            {
                var first = results
                    .Where(r => r.Resolution != null)
                    .SkipWhile(r =>
                    {
                        var v = (IList<Dictionary<string, string>>)r.Resolution["values"];
                        var returnValue = v.Any(x =>
                        {
                            try
                            {
                                return x["value"] == "not resolved";
                            }
                            catch (KeyNotFoundException)
                            {
                                return false;
                            }
                        });
                        return returnValue;
                    })
                    .FirstOrDefault();
                if (first == null)
                {
                    throw new EventParseException();
                }

                var resolutionValues = (IList<Dictionary<string, string>>)first.Resolution["values"];

                var subType = first.TypeName.Split('.').Last();
                if (subType == "date")
                {
                    string timex = resolutionValues.Select(v => v["timex"]).FirstOrDefault();
                    string value = resolutionValues.Select(v => v["value"]).FirstOrDefault();
                    if (timex.StartsWith("XXXX-"))
                    {
                        value = value.Substring(4);
                        value = string.Format("{0}{1}", DateTime.Now.Year, value);
                    }
                    LocalDateTime startDateTime = _dateOnlyPattern.Parse(value).Value;
                    ZonedDateTime zonedStartDateTime = tz.AtLeniently(startDateTime);
                    DateTimeOffset startTimestamp = zonedStartDateTime.ToDateTimeOffset();

                    if (timex.StartsWith("XXXX-") && !IsFuture(startTimestamp))
                    {
                        startTimestamp = startTimestamp.AddYears(1);
                    }

                    if (IsFuture(startTimestamp))
                    {
                        evt.StartTimestamp = startTimestamp;
                        evt.EndTimestamp = startTimestamp.AddHours(1);
                    }
                    else
                    {
                        throw new DateTimeInPastException();
                    }
                    
                }
                else if (subType.Contains("date") && !subType.Contains("range"))
                {
                    string timex = resolutionValues.Select(v => v["timex"]).FirstOrDefault();
                    string value = resolutionValues.Select(v => v["value"]).FirstOrDefault();
                    if (timex.StartsWith("XXXX-"))
                    {
                        value = value.Substring(4);
                        value = string.Format("{0}{1}", DateTime.Now.Year, value);
                    }
                    LocalDateTime startDateTime = _dateTimePattern.Parse(value).Value;
                    ZonedDateTime zonedStartDateTime = tz.AtLeniently(startDateTime);
                    DateTimeOffset startTimestamp = zonedStartDateTime.ToDateTimeOffset();

                    if (timex.StartsWith("XXXX-") && !IsFuture(startTimestamp))
                    {
                        startTimestamp = startTimestamp.AddYears(1);
                    }

                    if (IsFuture(startTimestamp))
                    {
                        evt.StartTimestamp = startTimestamp;
                        evt.EndTimestamp = startTimestamp.AddHours(1);
                    }
                    else
                    {
                        throw new DateTimeInPastException();
                    }
                }
                else if (subType.Contains("date") && subType.Contains("range"))
                {
                    string timex = resolutionValues.First()["timex"].TrimStart('(').TrimEnd(')');
                    string[] timexSplit = timex.Split(',');
                    string fromString = "";
                    string toString = "";

                    try
                    {
                        fromString = resolutionValues.First()["start"];
                        toString = resolutionValues.First()["end"];
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new EventParseException();
                    }

                    if (timexSplit[0].StartsWith("XXXX-"))
                    {
                        fromString = fromString.Substring(4);
                        fromString = string.Format("{0}{1}", DateTime.Now.Year, fromString);
                    }
                    Console.WriteLine(timexSplit.Length);
                    if (timexSplit.Length > 1 && timexSplit[1].StartsWith("XXXX-"))
                    {
                        toString = toString.Substring(4);
                        toString = string.Format("{0}{1}", DateTime.Now.Year, toString);
                    }

                    LocalDateTime from, to;
                    if (fromString.Length == 10)
                    {
                        from = _dateOnlyPattern.Parse(fromString).Value;
                    }
                    else
                    {
                        from = _dateTimePattern.Parse(fromString).Value;
                    }
                    if (toString.Length == 10)
                    {
                        to = _dateOnlyPattern.Parse(toString).Value;
                    }
                    else
                    {
                        to = _dateTimePattern.Parse(toString).Value;
                    }

                    ZonedDateTime zonedFrom = tz.AtLeniently(from);
                    ZonedDateTime zonedTo = tz.AtLeniently(to);

                    DateTimeOffset startTimestamp = zonedFrom.ToDateTimeOffset();
                    DateTimeOffset endTimestamp = zonedTo.ToDateTimeOffset();

                    if (timexSplit[0].StartsWith("XXXX-") && !IsFuture(startTimestamp))
                    {
                        startTimestamp = startTimestamp.AddYears(1);
                    }

                    if (timexSplit.Length > 1 && timexSplit[1].StartsWith("XXXX-") && !IsFuture(endTimestamp))
                    {
                        endTimestamp = endTimestamp.AddYears(1);
                    }


                    if (IsFuture(startTimestamp) && IsFuture(endTimestamp))
                    {
                        evt.StartTimestamp = startTimestamp;
                        evt.EndTimestamp = endTimestamp;
                        if (IsEventEndBeforeStart(evt))
                        {
                            throw new EventEndBeforeStartException();
                        }
                    }
                    else
                    {
                        throw new DateTimeInPastException();
                    }
                }
                else if (subType.Contains("time") && !subType.Contains("range"))
                {
                    var clock = SystemClock.Instance;
                    LocalDate today = clock.InZone(tz).GetCurrentDate();
                    string timeString = resolutionValues.Select(v => v["value"]).FirstOrDefault();
                    string dateTimeString = string.Format("{0} {1}", today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), timeString);

                    LocalDateTime startDateTime = _dateTimePattern.Parse(dateTimeString).Value;
                    ZonedDateTime zonedStartDateTime = tz.AtLeniently(startDateTime);

                    if (!IsFuture(zonedStartDateTime.ToDateTimeOffset()))
                    {
                        zonedStartDateTime = zonedStartDateTime.PlusHours(24);
                    }
                    evt.StartTimestamp = zonedStartDateTime.ToDateTimeOffset();
                    evt.EndTimestamp = zonedStartDateTime.ToDateTimeOffset().AddHours(1);
                }
                else if (subType.Contains("time") && subType.Contains("range"))
                {
                    var clock = SystemClock.Instance;
                    LocalDate today = clock.InZone(tz).GetCurrentDate();
                    string fromString = "";
                    string toString = "";

                    try
                    {
                        fromString = resolutionValues.First()["start"];
                        toString = resolutionValues.First()["end"];
                    }
                    catch (KeyNotFoundException)
                    {
                        throw new EventParseException();
                    }

                    string fromDateTimeString = string.Format("{0} {1}", today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), fromString);
                    string toDateTimeString = string.Format("{0} {1}", today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), toString);

                    LocalDateTime startDateTime = _dateTimePattern.Parse(fromDateTimeString).Value;
                    ZonedDateTime zonedStartDateTime = tz.AtLeniently(startDateTime);
                    LocalDateTime endDateTime = _dateTimePattern.Parse(toDateTimeString).Value;
                    ZonedDateTime zonedEndDateTime = tz.AtLeniently(endDateTime);
                    DateTimeOffset startTimestamp = zonedStartDateTime.ToDateTimeOffset();
                    DateTimeOffset endTimestamp = zonedEndDateTime.ToDateTimeOffset();

                    if (!IsFuture(startTimestamp) && !IsFuture(endTimestamp))
                    {
                        startTimestamp = startTimestamp.AddDays(1);
                        endTimestamp = endTimestamp.AddDays(1);
                    }

                    evt.StartTimestamp = startTimestamp;
                    evt.EndTimestamp = endTimestamp;
                    if (IsEventEndBeforeStart(evt))
                    {
                        throw new EventEndBeforeStartException();
                    }
                }
                else
                {
                    throw new EventParseException();
                }

                evt.ReminderTimestamp = null;
                newBodyString = bodyString.RemoveCaseInsensitive(first.Text);

                if (isUpdate)
                {
                    if (!string.IsNullOrEmpty(newBodyString))
                    {
                        // Update the event name
                        evt.Name = newBodyString;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(newBodyString))
                    {
                        // This is not fine. Name is required for creating new events
                        throw new EventParseException();
                    }
                    else
                    {
                        evt.Name = newBodyString;
                    }
                }
            }
            else
            {
                if (results.Count == 0 && isUpdate)
                {
                    // User has entered a new event name, so update the name
                    // Note: bodyString is assumed not null and not empty
                    evt.Name = bodyString;
                }
                else
                {
                    throw new EventParseException();
                }
            }

        }

        private static bool IsFuture(DateTimeOffset date)
        {
            return date > DateTimeOffset.Now;
        }

        private static bool IsEventEndBeforeStart(Event evt)
        {
            return evt.EndTimestamp <= evt.StartTimestamp;
        }
    }
}
