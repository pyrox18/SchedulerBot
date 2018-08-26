using System;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json;

namespace SchedulerBot.Data
{
    public class SchedulerBotContextFactory : IDesignTimeDbContextFactory<SchedulerBotContext>
    {
        public SchedulerBotContext CreateDbContext(string[] args)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var json = "";
            using (var fs = File.OpenRead($"appsettings.{environment}.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();

            var config = JsonConvert.DeserializeAnonymousType(json, new
            {
                ConnectionStrings = new
                {
                    SchedulerBotContext = ""
                }
            });

            var builder = new DbContextOptionsBuilder<SchedulerBotContext>();
            builder.UseNpgsql(config.ConnectionStrings.SchedulerBotContext);

            return new SchedulerBotContext(builder.Options);
        }
    }
}
