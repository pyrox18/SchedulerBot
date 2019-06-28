using MediatR;
using SchedulerBot.Application.Settings.Models;

namespace SchedulerBot.Application.Settings.Queries.GetAllSettings
{
    public class GetAllSettingsQuery : IRequest<SettingsViewModel>
    {
        public ulong CalendarId { get; set; }
    }
}