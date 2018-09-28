using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Quartz;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Factories;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Client.Scheduler
{
    public class EventNotifyJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                JobDataMap jobDataMap = context.MergedJobDataMap;
                var evt = (Event)jobDataMap["event"];
                var client = (DiscordClient)jobDataMap["client"];
                var channel = (DiscordChannel)jobDataMap["channel"];

                var embed = EventEmbedFactory.GetNotifyEventEmbed(evt);
                if (evt.Mentions != null)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var mention in evt.Mentions)
                    {
                        switch (mention.Type)
                        {
                            case Data.Models.MentionType.Role:
                                sb.Append($"{mention.TargetId.AsRoleMention()} ");
                                break;
                            case Data.Models.MentionType.User:
                                sb.Append($"{mention.TargetId.AsUserMention()} ");
                                break;
                            case Data.Models.MentionType.Everyone:
                                sb.Append("@everyone");
                                break;
                            case Data.Models.MentionType.RSVP:
                                foreach (var rsvp in evt.RSVPs)
                                {
                                    sb.Append($"{rsvp.UserId.AsUserMention()} ");
                                }
                                break;
                        }
                    }
                    await client.SendMessageAsync(channel, sb.ToString(), embed: embed);
                    return;
                }

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
