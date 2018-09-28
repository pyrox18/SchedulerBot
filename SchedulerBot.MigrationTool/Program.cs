using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using NodaTime;
using SchedulerBot.Data;
using SchedulerBot.Data.Models;

namespace SchedulerBot.MigrationTool
{
    class Program
    {
        public static IConfigurationRoot Configuration;
        public static MongoClient MongoClient;
        public static SchedulerBotContext PostgresContext;
        public static Dictionary<string, string> TimezoneCasePairs = new Dictionary<string, string>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("SchedulerBot Database Migration Tool (v1.0.x to v2.0.0)");

            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (String.IsNullOrWhiteSpace(environment))
            {
                throw new ArgumentNullException("Environment not found in ASPNETCORE_ENVIRONMENT");
            }

            Console.WriteLine($"Environment: {environment}");

            Console.WriteLine("Reading configuration file...");
            Configure(environment);

            Console.WriteLine("Connecting to Postgres server...");
            var contextFactory = new SchedulerBotContextFactory();
            PostgresContext = contextFactory.CreateDbContext(null);

            Console.WriteLine("Connecting to Mongo server...");
            MongoClient = new MongoClient(Configuration.GetConnectionString("MongoDb"));
            Console.WriteLine("Getting database...");
            IMongoDatabase mongoDb = MongoClient.GetDatabase("schedulerbot");

            Console.WriteLine("Getting calendar collection...");
            var calendarCollection = mongoDb.GetCollection<Documents.Calendar>("calendars");
            var calendars = calendarCollection.Find(new BsonDocument()).ToList();
            Console.WriteLine($"Found approximately {calendars.Count} calendar documents.");
            Console.WriteLine("Migrating...");
            Console.WriteLine($"0 out of {calendars.Count} calendars migrated.");
            int migrateCount = 0;
            foreach (var calendar in calendars)
            {
                MigrateCalendar(calendar, out Calendar newCalendar, out List<Permission> newPermissions);
                PostgresContext.Calendars.Add(newCalendar);
                PostgresContext.Permissions.AddRange(newPermissions);
                migrateCount++;
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.WriteLine($"{migrateCount} out of {calendars.Count} calendars migrated.");
            }
            Console.WriteLine("Saving to database...");
            PostgresContext.SaveChanges();
            Console.WriteLine("Migration completed.");

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void Configure(string environment)
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
            builder.AddJsonFile($"appsettings.{environment}.json");

            Configuration = builder.Build();
        }

        private static void ClearConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        private static void MigrateCalendar(Documents.Calendar oldCalendar, out Calendar calendar, out List<Permission> permissions)
        {
            calendar = new Calendar
            {
                Id = UInt64.Parse(oldCalendar.Id),
                Prefix = oldCalendar.Prefix,
                Events = new List<Event>()
            };

            if (!string.IsNullOrEmpty(oldCalendar.DefaultChannel))
            {
                calendar.DefaultChannel = UInt64.Parse(oldCalendar.DefaultChannel);
            }

            if (!string.IsNullOrEmpty(oldCalendar.Timezone))
            {
                var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(oldCalendar.Timezone);
                var defaultTz = DateTimeZoneProviders.Tzdb["UTC"];

                if (tz == null)
                {
                    var memorisedTimezone = TimezoneCasePairs.GetValueOrDefault(oldCalendar.Timezone.ToLower());
                    if (!string.IsNullOrEmpty(memorisedTimezone))
                    {
                        tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(memorisedTimezone);
                    }
                }
                while (tz == null)
                {
                    Console.WriteLine($"Timezone {oldCalendar.Timezone} not found for calendar {oldCalendar.Id}.");
                    Console.Write("Enter a suitable timezone: ");
                    var timezone = Console.ReadLine();
                    tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timezone);
                    if (tz != null)
                    {
                        TimezoneCasePairs.Add(oldCalendar.Timezone.ToLower(), timezone);
                    }
                    ClearConsoleLine();
                    Console.CursorTop--;
                    ClearConsoleLine();
                    Console.CursorTop--;
                }
                calendar.Timezone = tz.ToString();
                calendar.Events = new List<Event>();

                foreach (var evt in oldCalendar.Events)
                {
                    var newEvent = new Event
                    {
                        Id = Guid.NewGuid(),
                        Name = evt.Name,
                        Description = evt.Description
                    };

                    switch (evt.Repeat)
                    {
                        case "d":
                            newEvent.Repeat = RepeatType.Daily;
                            break;
                        case "w":
                            newEvent.Repeat = RepeatType.Weekly;
                            break;
                        case "m":
                            newEvent.Repeat = RepeatType.Monthly;
                            break;
                        default:
                            newEvent.Repeat = RepeatType.None;
                            break;
                    }

                    LocalDateTime startDt = LocalDateTime.FromDateTime(evt.StartTimestamp);
                    LocalDateTime endDt = LocalDateTime.FromDateTime(evt.EndTimestamp);
                    ZonedDateTime zonedStart = defaultTz.AtStrictly(startDt);
                    ZonedDateTime zonedEnd = defaultTz.AtStrictly(endDt);
                    newEvent.StartTimestamp = zonedStart.ToInstant().InZone(tz).ToDateTimeOffset();
                    newEvent.EndTimestamp = zonedEnd.ToInstant().InZone(tz).ToDateTimeOffset();

                    calendar.Events.Add(newEvent);
                }
            }

            permissions = new List<Permission>();
            foreach (var perm in oldCalendar.Permissions)
            {
                PermissionNode node;
                switch (perm.Node)
                {
                    case "event.create":
                        node = PermissionNode.EventCreate;
                        break;
                    case "event.list":
                        node = PermissionNode.EventList;
                        break;
                    case "event.update":
                        node = PermissionNode.EventUpdate;
                        break;
                    case "event.delete":
                        node = PermissionNode.EventDelete;
                        break;
                    case "ping":
                        node = PermissionNode.Ping;
                        break;
                    case "prefix.show":
                        node = PermissionNode.PrefixShow;
                        break;
                    case "prefix.modify":
                        node = PermissionNode.PrefixModify;
                        break;
                    case "defaultchannel.show":
                        node = PermissionNode.DefaultChannelShow;
                        break;
                    case "defaultchannel.modify":
                        node = PermissionNode.DefaultChannelModify;
                        break;
                    case "timezone.show":
                        node = PermissionNode.TimezoneShow;
                        break;
                    case "timezone.modify":
                        node = PermissionNode.TimezoneModify;
                        break;
                    case "perms.show":
                        node = PermissionNode.PermsShow;
                        break;
                    case "perms.modify":
                        node = PermissionNode.PermsModify;
                        break;
                    case "perms.nodes":
                        node = PermissionNode.PermsNodes;
                        break;
                    default:
                        continue;
                }

                foreach (var id in perm.DeniedRoles)
                {
                    var ulongId = UInt64.Parse(id);
                    permissions.Add(new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node,
                        IsDenied = true,
                        Type = PermissionType.Role,
                        TargetId = ulongId
                    });
                }

                foreach (var id in perm.DeniedUsers)
                {
                    var ulongId = UInt64.Parse(id);
                    permissions.Add(new Permission
                    {
                        Id = Guid.NewGuid(),
                        Calendar = calendar,
                        Node = node,
                        IsDenied = true,
                        Type = PermissionType.User,
                        TargetId = ulongId
                    });
                }
            }
        }
    }
}
