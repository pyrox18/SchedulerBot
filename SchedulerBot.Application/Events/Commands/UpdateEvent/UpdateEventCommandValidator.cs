using FluentValidation;

namespace SchedulerBot.Application.Events.Commands.UpdateEvent
{
    public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
    {
        public UpdateEventCommandValidator()
        {
            RuleFor(x => x.CalendarId)
                .NotEmpty();

            RuleFor(x => x.EventArgs)
                .NotEmpty();

            RuleFor(x => x.EventIndex)
                .GreaterThanOrEqualTo(0);
        }
    }
}
