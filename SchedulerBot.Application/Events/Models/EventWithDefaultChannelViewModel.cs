using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Events.Models
{
    public class EventWithDefaultChannelViewModel : EventViewModel
    {
        public EventWithDefaultChannelViewModel()
        {
        }

        public EventWithDefaultChannelViewModel(EventViewModel viewModel)
        {
            CalendarId = viewModel.CalendarId;
            Name = viewModel.Name;
            Description = viewModel.Description;
            StartTimestamp = viewModel.StartTimestamp;
            EndTimestamp = viewModel.EndTimestamp;
            ReminderTimestamp = viewModel.ReminderTimestamp;
            Repeat = viewModel.Repeat;
            Mentions = viewModel.Mentions;
            RSVPs = viewModel.RSVPs;
        }

        public ulong DefaultChannel { get; set; }

        public new static EventWithDefaultChannelViewModel FromEvent(Event @event)
        {
            return new EventWithDefaultChannelViewModel(EventViewModel.FromEvent(@event));
        }
    }
}