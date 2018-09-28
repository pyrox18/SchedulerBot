using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Quartz;
using SchedulerBot.Client.Factories;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Client.Scheduler
{
    public class EventReminderJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                JobDataMap jobDataMap = context.MergedJobDataMap;
                var evt = (Event)jobDataMap["event"];
                var client = (DiscordClient)jobDataMap["client"];
                var channel = (DiscordChannel)jobDataMap["channel"];

                var embed = EventEmbedFactory.GetRemindEventEmbed(evt);
                await client.SendMessageAsync(channel, embed: embed);
            }
            catch (UnauthorizedException) { }
            catch
            {
                throw;
            }
        }
    }
}
