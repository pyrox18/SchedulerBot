using MediatR;
using SchedulerBot.Application.Events.Models;
using System;

namespace SchedulerBot.Application.Events.Commands.ApplyEventRepeat
{
    public class ApplyEventRepeatCommand : IRequest<EventViewModel>
    {
        public Guid EventId { get; set; }
    }
}
