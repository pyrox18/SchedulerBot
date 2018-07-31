using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Quartz;
using SchedulerBot.Client.EmbedFactories;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Client.Scheduler
{
    public class EventNotifyJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap jobDataMap = context.MergedJobDataMap;
            var evt = (Event)jobDataMap["event"];
            var client = (DiscordClient)jobDataMap["client"];
            var channel = (DiscordChannel)jobDataMap["channel"];

            var embed = EventEmbedFactory.GetNotifyEventEmbed(evt);
            await client.SendMessageAsync(channel, embed: embed);
        }
    }
}
