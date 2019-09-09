using MediatR;
using System;

namespace SchedulerBot.Application.Events.Commands.DeleteEvent
{
    public class DeleteEventByIdCommand : IRequest
    {
        public Guid EventId { get; set; }
    }
}
