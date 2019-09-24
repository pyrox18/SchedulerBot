using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NodaTime;
using SchedulerBot.Application.Events.Models;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Domain.Models;

namespace SchedulerBot.Application.Events.Commands.ApplyEventRepeat
{
    public class ApplyEventRepeatCommandHandler : IRequestHandler<ApplyEventRepeatCommand, EventViewModel>
    {
        private readonly IEventRepository _eventRepository;

        public ApplyEventRepeatCommandHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<EventViewModel> Handle(ApplyEventRepeatCommand request, CancellationToken cancellationToken = default)
        {
            var @event = await _eventRepository.GetByIdAsync(request.EventId);

            @event.ApplyRepeat();

            await _eventRepository.UpdateAsync(@event);

            AdjustTimestampsToTimezone(@event, @event.Calendar.Timezone);

            return EventViewModel.FromEvent(@event);
        }

        private void AdjustTimestampsToTimezone(Event evt, string timezone)
        {
            var tz = DateTimeZoneProviders.Tzdb[timezone];
            Instant instant = Instant.FromDateTimeOffset(evt.StartTimestamp);
            LocalDateTime dt = new ZonedDateTime(instant, DateTimeZoneProviders.Tzdb[timezone]).LocalDateTime;
            ZonedDateTime zdt = tz.AtLeniently(dt);
            evt.StartTimestamp = zdt.ToDateTimeOffset();

            instant = Instant.FromDateTimeOffset(evt.EndTimestamp);
            dt = new ZonedDateTime(instant, DateTimeZoneProviders.Tzdb[timezone]).LocalDateTime;
            zdt = tz.AtLeniently(dt);
            evt.EndTimestamp = zdt.ToDateTimeOffset();

            if (evt.ReminderTimestamp != null)
            {
                instant = Instant.FromDateTimeOffset((DateTimeOffset)evt.ReminderTimestamp);
                dt = new ZonedDateTime(instant, DateTimeZoneProviders.Tzdb[timezone]).LocalDateTime;
                zdt = tz.AtLeniently(dt);
                evt.ReminderTimestamp = zdt.ToDateTimeOffset();
            }
        }
    }
}
