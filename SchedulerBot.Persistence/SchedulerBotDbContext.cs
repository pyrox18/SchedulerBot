using Microsoft.EntityFrameworkCore;
using SchedulerBot.Domain.Models;

namespace SchedulerBot.Persistence
{
    public class SchedulerBotDbContext : DbContext
    {
        public SchedulerBotDbContext(DbContextOptions<SchedulerBotDbContext> options)
            : base(options)
        { }

        public DbSet<Calendar> Calendars { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<EventMention> EventMentions { get; set; }
        public DbSet<EventRSVP> EventRSVPs { get; set; }
    }
}
