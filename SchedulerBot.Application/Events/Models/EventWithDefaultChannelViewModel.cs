using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Events.Models
{
    public class EventWithDefaultChannelViewModel : EventViewModel
    {
        public ulong DefaultChannel { get; set; }

        public EventWithDefaultChannelViewModel(Event @event) :
            base(@event)
        {
            DefaultChannel = @event.Calendar.DefaultChannel;
        }
    }
}
