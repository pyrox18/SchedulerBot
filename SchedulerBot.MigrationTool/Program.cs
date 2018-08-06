using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SchedulerBot.MigrationTool
{
    class Program
    {
        public static IConfigurationRoot Configuration;
        public static MongoClient MongoClient;
        

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

            Console.WriteLine("Connecting to Mongo server...");
            MongoClient = new MongoClient(Configuration.GetConnectionString("MongoDb"));
            Console.WriteLine("Getting database...");
            IMongoDatabase mongoDb = MongoClient.GetDatabase("schedulerbot");

            Console.WriteLine("Getting calendar collection...");
            var calendarCollection = mongoDb.GetCollection<Documents.Calendar>("calendars");
            var calendars = calendarCollection.Find(new BsonDocument()).ToList();
            Console.WriteLine($"Found approximately {calendars.Count} calendar documents.");

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
    }
}
