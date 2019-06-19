namespace SchedulerBot.Application.Calendars.Models
{
    public class InitialisedCalendarViewModel
    {
        public ulong CalendarId { get; set; }
        public string Timezone { get; set; }
        public ulong ChannelId { get; set; }
    }
}