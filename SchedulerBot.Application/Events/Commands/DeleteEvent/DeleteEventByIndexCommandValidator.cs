using FluentValidation;

namespace SchedulerBot.Application.Events.Commands.DeleteEvent
{
    public class DeleteEventByIndexCommandValidator : AbstractValidator<DeleteEventByIndexCommand>
    {
        public DeleteEventByIndexCommandValidator()
        {
            RuleFor(x => x.CalendarId)
                .NotEmpty();

            RuleFor(x => x.Index)
                .GreaterThanOrEqualTo(0);
        }
    }
}
