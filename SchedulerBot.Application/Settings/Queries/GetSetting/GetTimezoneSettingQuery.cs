using MediatR;
using SchedulerBot.Application.Settings.Models;

namespace SchedulerBot.Application.Settings.Queries.GetSetting
{
    public class GetTimezoneSettingQuery : IRequest<TimezoneSettingViewModel>
    {
        public ulong CalendarId { get; set; }
    }
}
