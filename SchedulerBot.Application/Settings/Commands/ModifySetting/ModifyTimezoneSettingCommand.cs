using MediatR;
using SchedulerBot.Application.Settings.Models;

namespace SchedulerBot.Application.Settings.Commands.ModifySetting
{
    public class ModifyTimezoneSettingCommand : IRequest<TimezoneSettingViewModel>
    {
        public ulong CalendarId { get; set; }
        public string NewTimezone { get; set; }
    }
}
