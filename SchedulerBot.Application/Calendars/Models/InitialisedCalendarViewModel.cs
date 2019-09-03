using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Calendars.Models
{
    public class InitialisedCalendarViewModel
    {
        public ulong CalendarId { get; set; }
        public string Timezone { get; set; }
        public ulong ChannelId { get; set; }

        public static InitialisedCalendarViewModel FromCalendar(Calendar calendar)
        {
            return new InitialisedCalendarViewModel
            {
                CalendarId = calendar.Id,
                Timezone = calendar.Timezone,
                ChannelId = calendar.DefaultChannel
            };
        }
    }
}