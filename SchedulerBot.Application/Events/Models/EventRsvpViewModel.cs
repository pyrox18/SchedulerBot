using SchedulerBot.Domain.Models;

namespace SchedulerBot.Application.Events.Models
{
    public class EventRsvpViewModel : EventViewModel
    {
        public EventRsvpViewModel()
        {
        }

        public EventRsvpViewModel(EventViewModel viewModel)
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
        public bool RsvpAdded { get; set; }

        public new static EventRsvpViewModel FromEvent(Event @event)
        {
            return new EventRsvpViewModel(EventViewModel.FromEvent(@event));
        }
    }
}
