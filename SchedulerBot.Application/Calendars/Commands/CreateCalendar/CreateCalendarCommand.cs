using MediatR;

namespace SchedulerBot.Application.Calendars.Commands.CreateCalendar
{
    public class CreateCalendarCommand : IRequest
    {
        public ulong CalendarId { get; set; }
        public string Prefix { get; set; }
    }
}
