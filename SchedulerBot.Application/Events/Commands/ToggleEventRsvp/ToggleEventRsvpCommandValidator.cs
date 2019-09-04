using FluentValidation;

namespace SchedulerBot.Application.Events.Commands.ToggleEventRsvp
{
    public class ToggleEventRsvpCommandValidator : AbstractValidator<ToggleEventRsvpCommand>
    {
        public ToggleEventRsvpCommandValidator()
        {
            RuleFor(x => x.CalendarId)
                .NotEmpty();

            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.Index)
                .GreaterThanOrEqualTo(0);
        }
    }
}
