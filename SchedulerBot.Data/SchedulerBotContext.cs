using Microsoft.EntityFrameworkCore;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Data
{
    public class SchedulerBotContext : DbContext
    {
        public SchedulerBotContext(DbContextOptions<SchedulerBotContext> options)
            : base(options)
        { }

        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<EventMention> EventMentions { get; set; }
        public DbSet<EventRSVP> EventRSVPs { get; set; }
    }
}
