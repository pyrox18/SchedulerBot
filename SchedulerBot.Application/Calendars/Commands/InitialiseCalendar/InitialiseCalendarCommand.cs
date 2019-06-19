using MediatR;
using SchedulerBot.Application.Calendars.Models;

namespace SchedulerBot.Application.Calendars.Commands.InitialiseCalendar
{
    public class InitialiseCalendarCommand : IRequest<InitialisedCalendarViewModel>
    {
        public ulong CalendarId { get; set; }
        public string Timezone { get; set; }
        public ulong ChannelId { get; set; }
        public string Prefix { get; set; }
    }
}