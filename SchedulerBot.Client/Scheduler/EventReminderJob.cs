using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Quartz;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Factories;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Client.Scheduler
{
    public class EventReminderJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap jobDataMap = context.MergedJobDataMap;
            var evt = (Event)jobDataMap["event"];
            var client = (DiscordClient)jobDataMap["client"];
            var channel = (DiscordChannel)jobDataMap["channel"];

            var embed = EventEmbedFactory.GetRemindEventEmbed(evt);
            await client.SendMessageAsync(channel, embed: embed);
        }
    }
}
