using MediatR;
using SchedulerBot.Application.Settings.Models;

namespace SchedulerBot.Application.Settings.Queries.GetSetting
{
    public class GetPrefixSettingQuery : IRequest<PrefixSettingViewModel>
    {
        public ulong CalendarId { get; set; }
    }
}