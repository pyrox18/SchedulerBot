using MediatR;
using SchedulerBot.Application.Events.Models;
using System;

namespace SchedulerBot.Application.Events.Queries.GetEvent
{
    public class GetEventByIdQuery : IRequest<EventViewModel>
    {
        public Guid EventId { get; set; }
    }
}
