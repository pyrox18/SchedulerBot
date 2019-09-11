using MediatR;

namespace SchedulerBot.Application.Calendars.Commands.DeleteCalendar
{
    public class DeleteCalendarCommand : IRequest
    {
        public ulong CalendarId { get; set; }
    }
}
