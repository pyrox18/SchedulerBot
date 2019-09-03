using FluentValidation;
using NodaTime;

namespace SchedulerBot.Application.Calendars.Commands.InitialiseCalendar
{
    public class InitialiseCalendarCommandValidator : AbstractValidator<InitialiseCalendarCommand>
    {
        public InitialiseCalendarCommandValidator()
        {
            RuleFor(x => x.CalendarId)
                .NotEmpty();

            RuleFor(x => x.ChannelId)
                .NotEmpty();

            RuleFor(x => x.Prefix)
                .NotEmpty();

            RuleFor(x => x.Timezone)
                .Must(tz => !(DateTimeZoneProviders.Tzdb.GetZoneOrNull(tz) is null))
                .WithMessage("Timezone must exist in the tz database");
        }
    }
}
