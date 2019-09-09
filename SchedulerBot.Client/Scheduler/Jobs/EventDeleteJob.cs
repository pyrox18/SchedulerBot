using System.Threading.Tasks;
using MediatR;
using Quartz;
using SchedulerBot.Application.Events.Commands.DeleteEvent;
using SchedulerBot.Client.Scheduler.Data;

namespace SchedulerBot.Client.Scheduler.Jobs
{
    public class EventDeleteJob : IJob
    {
        private readonly IMediator _mediator;

        public EventDeleteJob(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var data = context.MergedJobDataMap as EventNotifyJobDataMap;

            await _mediator.Send(new DeleteEventByIdCommand
            {
                EventId = data.EventId
            });
        }
    }
}
