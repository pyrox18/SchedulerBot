using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using SchedulerBot.Application.Events.Queries.GetEvents;

namespace SchedulerBot.Client.Scheduler.Jobs
{
    public class EventPollingJob : IJob
    {
        private readonly IMediator _mediator;
        private readonly DiscordShardedClient _shardedClient;
        private readonly ILogger<EventPollingJob> _logger;
        private readonly IEventScheduler _eventScheduler;

        public EventPollingJob(IMediator mediator, DiscordShardedClient shardedClient, ILogger<EventPollingJob> logger, IEventScheduler eventScheduler)
        {
            _mediator = mediator;
            _shardedClient = shardedClient;
            _logger = logger;
            _eventScheduler = eventScheduler;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var tasks = new List<Task>();
            foreach (var shardClient in _shardedClient.ShardClients)
            {
                tasks.Add(PollAndScheduleEventsForShard(shardClient.Key));
            }

            await Task.WhenAll(tasks);
        }

        private async Task PollAndScheduleEventsForShard(int clientShardId)
        {
            _logger.LogInformation($"Polling and scheduling events for shard {clientShardId}");

            var calendarIds = _shardedClient.ShardClients[clientShardId].Guilds.Keys;
            var query = new GetEventsInIntervalForCalendarsQuery
            {
                CalendarIds = calendarIds,
                Interval = new TimeSpan(2, 0, 0)
            };

            var result = await _mediator.Send(query);

            var tasks = new List<Task>();
            foreach (var @event in result)
            {
                tasks.Add(_eventScheduler.ScheduleEvent(@event, clientShardId, @event.DefaultChannel));
            }

            await Task.WhenAll(tasks);
        }
    }
}
