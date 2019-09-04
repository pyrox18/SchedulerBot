using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Specifications
{
    public class CalendarEventSpecification : BaseSpecification<Event>
    {
        // TODO: Remove includeCalendar when Event model is updated to have CalendarId property
        public CalendarEventSpecification(ulong calendarId, bool includeCalendar = false) :
            base(e => e.Calendar.Id == calendarId)
        {
            if (includeCalendar)
            {
                AddInclude(e => e.Calendar);
            }

            AddInclude(e => e.Mentions);
            AddInclude(e => e.RSVPs);
        }
    }
}
