using Microsoft.EntityFrameworkCore;
using SchedulerBot.Persistence.Infrastructure;

namespace SchedulerBot.Persistence
{
    public class SchedulerBotDbContextFactory : DesignTimeDbContextFactoryBase<SchedulerBotDbContext>
    {
        protected override SchedulerBotDbContext CreateNewInstance(DbContextOptions<SchedulerBotDbContext> options)
        {
            return new SchedulerBotDbContext(options);
        }
    }
}
