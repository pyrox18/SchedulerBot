using MediatR;
using SchedulerBot.Application.Settings.Models;

namespace SchedulerBot.Application.Settings.Queries.GetSetting
{
    public class GetDefaultChannelSettingQuery : IRequest<DefaultChannelSettingViewModel>
    {
        public ulong CalendarId { get; set; }
    }
}
