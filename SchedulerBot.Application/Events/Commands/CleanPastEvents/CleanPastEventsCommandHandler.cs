using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Application.Specifications;
using SchedulerBot.Domain.Enumerations;
using SchedulerBot.Domain.Models;

namespace SchedulerBot.Application.Events.Commands.CleanPastEvents
{
    public class CleanPastEventsCommandHandler : IRequestHandler<CleanPastEventsCommand>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IDateTimeOffset _dateTimeOffset;

        public CleanPastEventsCommandHandler(IEventRepository eventRepository, IDateTimeOffset dateTimeOffset)
        {
            _eventRepository = eventRepository;
            _dateTimeOffset = dateTimeOffset;
        }

        public async Task<Unit> Handle(CleanPastEventsCommand request, CancellationToken cancellationToken = default)
        {
            var now = _dateTimeOffset.Now;
            var spec = new BeforeTimestampEventSpecification(now);
            var events = await _eventRepository.ListAsync(spec);

            foreach (var evt in events)
            {
                if (evt.Repeat == RepeatType.None)
                {
                    await _eventRepository.DeleteAsync(evt);
                }
                else
                {
                    // While loop used to perform repeated repeats until start is after current time
                    while (evt.StartTimestamp < now)
                    {
                        evt.ApplyRepeat();
                    }

                    await _eventRepository.UpdateAsync(evt);
                }
            }

            return Unit.Value;
        }
    }
}
