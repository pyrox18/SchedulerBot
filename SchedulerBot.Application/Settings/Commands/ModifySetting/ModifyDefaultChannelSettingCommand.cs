using MediatR;
using SchedulerBot.Application.Settings.Models;

namespace SchedulerBot.Application.Settings.Commands.ModifySetting
{
    public class ModifyDefaultChannelSettingCommand : IRequest<DefaultChannelSettingViewModel>
    {
        public ulong CalendarId { get; set; }
        public ulong NewDefaultChannel { get; set; }
    }
}
