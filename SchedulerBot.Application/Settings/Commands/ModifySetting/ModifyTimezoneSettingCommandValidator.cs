using FluentValidation;
using NodaTime;

namespace SchedulerBot.Application.Settings.Commands.ModifySetting
{
    public class ModifyTimezoneSettingCommandValidator : AbstractValidator<ModifyTimezoneSettingCommand>
    {
        public ModifyTimezoneSettingCommandValidator()
        {
            RuleFor(x => x.CalendarId)
                .NotEmpty();

            RuleFor(x => x.NewTimezone)
                .Must(tz => !(DateTimeZoneProviders.Tzdb.GetZoneOrNull(tz) is null))
                .WithMessage("Timezone must exist in the tz database");
        }
    }
}
