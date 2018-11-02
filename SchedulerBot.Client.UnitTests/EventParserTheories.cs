using System;
using System.Collections.Generic;
using Xunit;
using SchedulerBot.Client.Parsers;
using SchedulerBot.Data.Models;
using SchedulerBot.Client.Exceptions;

namespace SchedulerBot.Client.UnitTests
{
    public class EventParserTheories
    {
        public class ParseNewEventMethod
        {

            public static IEnumerable<object[]> NewEventSuccessTestData => new List<object[]>
            {
                new object[]
                {
                    "Test Event 9pm",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Test Event",
                        StartTimestamp = GetDateTimeOffsetTodayOrTomorrow(21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetTodayOrTomorrow(21, 0, 0, new TimeSpan(8, 0, 0)).AddHours(1),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                }
            };

            public static IEnumerable<object[]> NewEventParseExceptionTestData => new List<object[]>
            {
                new object[] { "Test Event" }
            };

            public static IEnumerable<object[]> NewEventTimezoneExceptionTestData => new List<object[]>
            {
                new object[] { "someInvalidTimezone" },
                new object[] { "gmt" }
            };

            [Theory]
            [MemberData(nameof(NewEventSuccessTestData))]
            public void ReturnsNewEvent(string args, string timezone, Event expected)
            {
                var result = EventParser.ParseNewEvent(args.Split(' '), timezone);

                Assert.Equal(expected.Name, result.Name);
                Assert.Equal(expected.StartTimestamp, result.StartTimestamp);
                Assert.Equal(expected.EndTimestamp, result.EndTimestamp);
                Assert.Equal(expected.ReminderTimestamp, result.ReminderTimestamp);
                Assert.Equal(expected.Description, result.Description);
                Assert.Equal(expected.Repeat, result.Repeat);
                try
                {
                    Assert.Empty(result.RSVPs);
                }
                catch (ArgumentNullException)
                {
                    Assert.Null(result.RSVPs);
                }

                if (expected.Mentions == null || expected.Mentions.Count == 0)
                {
                    try
                    {
                        Assert.Empty(result.Mentions);
                    }
                    catch (ArgumentNullException)
                    {
                        Assert.Null(result.Mentions);
                    }
                }
                else
                {
                    Assert.Equal(expected.Mentions.Count, result.Mentions.Count);
                    foreach (var expectedMention in expected.Mentions)
                    {
                        Assert.Contains(result.Mentions, m =>
                        {
                            return m.TargetId == expectedMention.TargetId
                                && m.Type == expectedMention.Type;
                        });
                    }
                }
            }

            [Theory]
            [MemberData(nameof(NewEventParseExceptionTestData))]
            public void ThrowsEventParseException(string args)
            {
                Assert.Throws<EventParseException>(() =>
                {
                    EventParser.ParseNewEvent(args.Split(' '), "Europe/London");
                });
            }

            [Theory]
            [MemberData(nameof(NewEventTimezoneExceptionTestData))]
            public void ThrowsInvalidTimezoneException(string timezone)
            {
                Assert.Throws<InvalidTimeZoneException>(() =>
                {
                    EventParser.ParseNewEvent("Test Event 8pm".Split(' '), timezone);
                });
            }
        }

        private static DateTimeOffset GetDateTimeOffsetTodayOrTomorrow(int hour, int minute, int second, TimeSpan timespan)
        {
            var now = DateTimeOffset.Now;
            var timestamp = new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, DateTimeOffset.Now.Day, hour, minute, second, timespan);
            if (timestamp <= now)
            {
                timestamp = timestamp.AddDays(1);
            }
            return timestamp;
        }
    }
}
