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
                // General mixed cases
                new object[]
                {
                    "Test Event 9pm",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Test Event",
                        StartTimestamp = GetDateTimeOffsetFuture(21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(21, 0, 0, new TimeSpan(8, 0, 0)).AddHours(1),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Some Event 3pm to 5pm --repeat d --mention <@!12345> --remind 15 minutes",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event",
                        StartTimestamp = GetDateTimeOffsetFuture(15, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(17, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = GetDateTimeOffsetFuture(15, 0, 0, new TimeSpan(8, 0, 0)).AddMinutes(-15),
                        Description = null,
                        Repeat = RepeatType.Daily,
                        Mentions = new List<EventMention>
                        {
                            new EventMention
                            {
                                Type = MentionType.User,
                                TargetId = 12345
                            }
                        }
                    }
                },
                new object[]
                {
                    "CSGO Scrims 7 July 10p --repeat w",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "CSGO Scrims",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 7, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 7, 23, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.Weekly,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Some Winter Event November 1st 9am-12pm --desc Very cold winter event",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Winter Event",
                        StartTimestamp = GetDateTimeOffsetFuture(11, 1, 9, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(11, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = "Very cold winter event",
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "1 Jan New Year Raid --desc First raid of the year --mention @everyone",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "New Year Raid",
                        StartTimestamp = GetDateTimeOffsetFuture(1, 1, 0, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(1, 1, 1, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = "First raid of the year",
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>
                        {
                            new EventMention
                            {
                                Type = MentionType.Everyone
                            }
                        }
                    }
                },
                // Repeat flag cases
                new object[]
                {
                    "Some Repeated Event 12 February 9pm-10pm",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Repeated Event",
                        StartTimestamp = GetDateTimeOffsetFuture(2, 12, 21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(2, 12, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Some Repeated Event 12 February 9pm-10pm --repeat d",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Repeated Event",
                        StartTimestamp = GetDateTimeOffsetFuture(2, 12, 21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(2, 12, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.Daily,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Some Repeated Event 12 February 9pm-10pm --repeat w",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Repeated Event",
                        StartTimestamp = GetDateTimeOffsetFuture(2, 12, 21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(2, 12, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.Weekly,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Some Repeated Event 12 February 9pm-10pm --repeat m",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Repeated Event",
                        StartTimestamp = GetDateTimeOffsetFuture(2, 12, 21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(2, 12, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.Monthly,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Some Repeated Event 12 February 9pm-10pm --repeat mw",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Repeated Event",
                        StartTimestamp = GetDateTimeOffsetFuture(2, 12, 21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(2, 12, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.MonthlyWeekday,
                        Mentions = new List<EventMention>()
                    }
                },
                // Mention cases
                new object[]
                {
                    "Some Event with Mentions 12 February 9pm-10pm --mention @everyone",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event with Mentions",
                        StartTimestamp = GetDateTimeOffsetFuture(2, 12, 21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(2, 12, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>
                        {
                            new EventMention
                            {
                                Type = MentionType.Everyone
                            }
                        }
                    }
                },
                new object[]
                {
                    "Some Event with Mentions 12 February 9pm-10pm --mention rsvp",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event with Mentions",
                        StartTimestamp = GetDateTimeOffsetFuture(2, 12, 21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(2, 12, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>
                        {
                            new EventMention
                            {
                                Type = MentionType.RSVP
                            }
                        }
                    }
                },
                new object[]
                {
                    "Some Event with Mentions 12 February 9pm-10pm --mention <@12345>",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event with Mentions",
                        StartTimestamp = GetDateTimeOffsetFuture(2, 12, 21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(2, 12, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>
                        {
                            new EventMention
                            {
                                Type = MentionType.User,
                                TargetId = 12345
                            }
                        }
                    }
                },
                new object[]
                {
                    "Some Event with Mentions 12 February 9pm-10pm --mention <@!12345>",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event with Mentions",
                        StartTimestamp = GetDateTimeOffsetFuture(2, 12, 21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(2, 12, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>
                        {
                            new EventMention
                            {
                                Type = MentionType.User,
                                TargetId = 12345
                            }
                        }
                    }
                },
                new object[]
                {
                    "Some Event with Mentions 12 February 9pm-10pm --mention <@&12345>",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event with Mentions",
                        StartTimestamp = GetDateTimeOffsetFuture(2, 12, 21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(2, 12, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>
                        {
                            new EventMention
                            {
                                Type = MentionType.Role,
                                TargetId = 12345
                            }
                        }
                    }
                },
                new object[]
                {
                    "Some Event with Mentions 12 February 9pm-10pm --mention <@&12345> <@24121> <@!24341> rsvp",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event with Mentions",
                        StartTimestamp = GetDateTimeOffsetFuture(2, 12, 21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(2, 12, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>
                        {
                            new EventMention
                            {
                                Type = MentionType.Role,
                                TargetId = 12345
                            },
                            new EventMention
                            {
                                Type = MentionType.User,
                                TargetId = 24121
                            },
                            new EventMention
                            {
                                Type = MentionType.User,
                                TargetId = 24341
                            },
                            new EventMention
                            {
                                Type = MentionType.RSVP
                            }
                        }
                    }
                },
                new object[]
                {
                    "Some Event with Mentions 12 February 9pm-10pm --mention <@&12345> <@24121> <@!24341> @everyone rsvp",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event with Mentions",
                        StartTimestamp = GetDateTimeOffsetFuture(2, 12, 21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(2, 12, 22, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>
                        {
                            new EventMention
                            {
                                Type = MentionType.Everyone
                            }
                        }
                    }
                },
                // Reminder cases
                new object[]
                {
                    "Some Event with Reminder July 1 12pm --remind 1 minute",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event with Reminder",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = GetDateTimeOffsetFuture(7, 1, 11, 59, 0, new TimeSpan(8, 0, 0)),
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Some Event with Reminder July 1 12pm --remind 90 minutes",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event with Reminder",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = GetDateTimeOffsetFuture(7, 1, 10, 30, 0, new TimeSpan(8, 0, 0)),
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Some Event with Reminder July 1 12pm --remind 3 hours",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event with Reminder",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = GetDateTimeOffsetFuture(7, 1, 9, 0, 0, new TimeSpan(8, 0, 0)),
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Some Event with Reminder July 1 12pm --remind 2 days",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event with Reminder",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = GetDateTimeOffsetFuture(6, 29, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                // Cases with ignored bad flags
                new object[]
                {
                    "Ignored Bad Flags Jul 1 12pm --desc",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Ignored Bad Flags",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Ignored Bad Flags Jul 1 12pm --remind",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Ignored Bad Flags",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Ignored Bad Flags Jul 1 12pm --remind apples",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Ignored Bad Flags",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Ignored Bad Flags Jul 1 12pm --repeat",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Ignored Bad Flags",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Ignored Bad Flags Jul 1 12pm --repeat a",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Ignored Bad Flags",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Ignored Bad Flags Jul 1 12pm --mention",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Ignored Bad Flags",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Ignored Bad Flags Jul 1 12pm --mention notAMention",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Ignored Bad Flags",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Ignored Bad Flags Jul 1 12pm --mention <@fakeMention> <@!fakeMention> <@&fakeMention>",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Ignored Bad Flags",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                },
                new object[]
                {
                    "Ignored Bad Flags Jul 1 12pm --notreallyaflag someValue",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Ignored Bad Flags",
                        StartTimestamp = GetDateTimeOffsetFuture(7, 1, 12, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(7, 1, 13, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = null,
                        Description = null,
                        Repeat = RepeatType.None,
                        Mentions = new List<EventMention>()
                    }
                }
            };

            public static IEnumerable<object[]> NewEventParseExceptionTestData => new List<object[]>
            {
                new object[] { "Test Event" },
                new object[] { "February 20th 9pm" },
                new object[] { "--desc Some description" },
                new object[] { "--repeat m" },
                new object[] { "--remind 15 minutes" },
                new object[] { "--mention rsvp" }
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

        public class ParseUpdateEventMethod
        {
            public static List<object[]> UpdateEventSuccessTestData => new List<object[]>
            {
                new object[]
                {
                    "Test Event 9pm",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Test Event",
                        StartTimestamp = GetDateTimeOffsetFuture(21, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(21, 0, 0, new TimeSpan(8, 0, 0)).AddHours(1),
                        ReminderTimestamp = null,
                        Description = "Base event description",
                        Repeat = RepeatType.Daily,
                        Mentions = new List<EventMention>
                        {
                            new EventMention
                            {
                                Type = MentionType.User,
                                TargetId = 12345
                            }
                        },
                        RSVPs = new List<EventRSVP>
                        {
                            new EventRSVP
                            {
                                UserId = 12345
                            }
                        }
                    }
                },
                new object[]
                {
                    "Some Event 3pm to 5pm --repeat w --mention <@!23456> --remind 15 minutes",
                    "Asia/Kuala_Lumpur",
                    new Event
                    {
                        Name = "Some Event",
                        StartTimestamp = GetDateTimeOffsetFuture(15, 0, 0, new TimeSpan(8, 0, 0)),
                        EndTimestamp = GetDateTimeOffsetFuture(17, 0, 0, new TimeSpan(8, 0, 0)),
                        ReminderTimestamp = GetDateTimeOffsetFuture(14, 45, 0, new TimeSpan(8, 0, 0)),
                        Description = "Base event description",
                        Repeat = RepeatType.Weekly,
                        Mentions = new List<EventMention>
                        {
                            new EventMention
                            {
                                Type = MentionType.User,
                                TargetId = 23456
                            }
                        },
                        RSVPs = new List<EventRSVP>
                        {
                            new EventRSVP
                            {
                                UserId = 12345
                            }
                        }
                    }
                }
            };

            [Theory]
            [MemberData(nameof(UpdateEventSuccessTestData))]
            public void ReturnsUpdatedEvent(string args, string timezone, Event expected)
            {
                var baseEvent = new Event
                {
                    Name = "Base Event",
                    StartTimestamp = new DateTimeOffset(2018, 7, 15, 12, 0, 0, new TimeSpan(8, 0, 0)),
                    EndTimestamp = new DateTimeOffset(2018, 7, 15, 3, 0, 0, new TimeSpan(8, 0, 0)),
                    ReminderTimestamp = new DateTimeOffset(2018, 7, 15, 11, 45, 0, new TimeSpan(8, 0, 0)),
                    Description = "Base event description",
                    Repeat = RepeatType.Daily,
                    Mentions = new List<EventMention>
                    {
                        new EventMention
                        {
                            Type = MentionType.User,
                            TargetId = 12345
                        }
                    },
                    RSVPs = new List<EventRSVP>
                    {
                        new EventRSVP
                        {
                            UserId = 12345
                        }
                    }
                };

                var result = EventParser.ParseUpdateEvent(baseEvent, args, timezone);

                Assert.Equal(expected.Name, result.Name);
                Assert.Equal(expected.StartTimestamp, result.StartTimestamp);
                Assert.Equal(expected.EndTimestamp, result.EndTimestamp);
                Assert.Equal(expected.ReminderTimestamp, result.ReminderTimestamp);
                Assert.Equal(expected.Description, result.Description);
                Assert.Equal(expected.Repeat, result.Repeat);
                Assert.Single(result.RSVPs);

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
        }

        private static DateTimeOffset GetDateTimeOffsetFuture(int hour, int minute, int second, TimeSpan timespan)
        {
            var now = DateTimeOffset.Now;
            var timestamp = new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, DateTimeOffset.Now.Day, hour, minute, second, timespan);
            if (timestamp <= now)
            {
                timestamp = timestamp.AddDays(1);
            }
            return timestamp;
        }

        private static DateTimeOffset GetDateTimeOffsetFuture(int month, int day, int hour, int minute, int second, TimeSpan timespan)
        {
            var now = DateTimeOffset.Now;
            var timestamp = new DateTimeOffset(DateTimeOffset.Now.Year, month, day, hour, minute, second, timespan);
            if (timestamp <= now)
            {
                timestamp = timestamp.AddYears(1);
            }
            return timestamp;
        }
    }
}
