using SchedulerBot.Domain.Models;

namespace SchedulerBot.Application.Settings.Models
{
    public class SettingsViewModel
    {
        public string Prefix { get; set; }
        public ulong DefaultChannel { get; set; }
        public string Timezone { get; set; }

        public static SettingsViewModel FromCalendar(Calendar calendar)
        {
            return new SettingsViewModel
            {
                Prefix = calendar.Prefix,
                DefaultChannel = calendar.DefaultChannel,
                Timezone = calendar.Timezone
            };
        }
    }
}