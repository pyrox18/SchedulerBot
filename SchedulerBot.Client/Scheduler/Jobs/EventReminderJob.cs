using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Exceptions;
using MediatR;
using Quartz;
using SchedulerBot.Application.Events.Queries.GetEvent;
using SchedulerBot.Client.Factories;
using SchedulerBot.Client.Scheduler.Data;

namespace SchedulerBot.Client.Scheduler.Jobs
{
    [DisallowConcurrentExecution]
    public class EventReminderJob : IJob
    {
        private readonly IMediator _mediator;
        private readonly DiscordShardedClient _shardedClient;

        public EventReminderJob(IMediator mediator, DiscordShardedClient shardedClient)
        {
            _mediator = mediator;
            _shardedClient = shardedClient;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var data = new EventNotifyJobDataMap(context.MergedJobDataMap);

            var @event = await _mediator.Send(new GetEventByIdQuery
            {
                EventId = data.EventId
            });

            var channel = await _shardedClient.ShardClients[data.ShardClientId].GetChannelAsync(data.ChannelId);

            var embed = EventEmbedFactory.GetRemindEventEmbed(@event);

            try
            {
                await channel.SendMessageAsync(embed: embed);
            }
            catch (UnauthorizedException) { }
        }
    }
}
