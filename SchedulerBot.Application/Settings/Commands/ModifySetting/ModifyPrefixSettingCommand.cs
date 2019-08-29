using MediatR;
using SchedulerBot.Application.Settings.Models;

namespace SchedulerBot.Application.Settings.Commands.ModifySetting
{
    public class ModifyPrefixSettingCommand : IRequest<PrefixSettingViewModel>
    {
        public ulong CalendarId { get; set; }
        public string NewPrefix { get; set; }
    }
}
