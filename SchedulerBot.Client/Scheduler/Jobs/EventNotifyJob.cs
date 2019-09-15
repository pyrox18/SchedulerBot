using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Exceptions;
using MediatR;
using Quartz;
using SchedulerBot.Application.Events.Queries.GetEvent;
using SchedulerBot.Client.Extensions;
using SchedulerBot.Client.Factories;
using SchedulerBot.Client.Scheduler.Data;

namespace SchedulerBot.Client.Scheduler.Jobs
{
    [DisallowConcurrentExecution]
    public class EventNotifyJob : IJob
    {
        private readonly IMediator _mediator;
        private readonly DiscordShardedClient _shardedClient;

        public EventNotifyJob(IMediator mediator, DiscordShardedClient shardedClient)
        {
            _mediator = mediator;
            _shardedClient = shardedClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var data = new EventJobDataMap(context.MergedJobDataMap);

            var @event = await _mediator.Send(new GetEventByIdQuery
            {
                EventId = data.EventId
            });

            var channel = await _shardedClient.ShardClients[data.ShardClientId].GetChannelAsync(data.ChannelId);

            var embed = EventEmbedFactory.GetNotifyEventEmbed(@event);

            try
            {
                if (@event.Mentions != null)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var mention in @event.Mentions)
                    {
                        switch (mention.Type)
                        {
                            case SchedulerBot.Domain.Models.MentionType.Role:
                                sb.Append($"{mention.TargetId.AsRoleMention()} ");
                                break;
                            case SchedulerBot.Domain.Models.MentionType.User:
                                sb.Append($"{mention.TargetId.AsUserMention()} ");
                                break;
                            case SchedulerBot.Domain.Models.MentionType.Everyone:
                                sb.Append("@everyone");
                                break;
                            case SchedulerBot.Domain.Models.MentionType.RSVP:
                                foreach (var rsvp in @event.RSVPs)
                                {
                                    sb.Append($"{rsvp.UserId.AsUserMention()} ");
                                }
                                break;
                        }
                    }

                    await channel.SendMessageAsync(sb.ToString(), embed: embed);
                }
                else
                {
                    await channel.SendMessageAsync(embed: embed);
                }
            }
            catch (UnauthorizedException) { }
        }
    }
}
