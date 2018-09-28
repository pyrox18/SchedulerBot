using System.Collections.Generic;

namespace SchedulerBot.Data.Models
{
    public class Calendar
    {
        public ulong Id { get; set; }
        public ulong DefaultChannel { get; set; }
        public string Prefix { get; set; }
        public string Timezone { get; set; }
        public List<Event> Events { get; set; }
    }
}
