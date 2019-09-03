using FluentValidation;

namespace SchedulerBot.Application.Events.Queries.GetEvent
{
    public class GetEventByIndexQueryValidator : AbstractValidator<GetEventByIndexQuery>
    {
        public GetEventByIndexQueryValidator()
        {
            RuleFor(x => x.CalendarId)
                .NotEmpty();

            RuleFor(x => x.Index)
                .GreaterThanOrEqualTo(0);
        }
    }
}
