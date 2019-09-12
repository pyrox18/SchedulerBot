using System.Collections.Generic;

namespace SchedulerBot.Client.Configuration
{
    public class BotConfiguration
    {
        public string Token { get; set; }
        public List<string> Prefixes { get; set; }
        public string Status { get; set; }
        public BotLinksConfiguration Links { get; set; }
    }

    public class BotLinksConfiguration
    {
        public string BotInvite { get; set; }
        public string SupportServer { get; set; }
        public string TimezoneList { get; set; }
    }
}
