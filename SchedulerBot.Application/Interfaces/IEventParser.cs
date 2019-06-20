using SchedulerBot.Data.Models;

namespace SchedulerBot.Application.Interfaces
{
    public interface IEventParser
    {
        Event ParseNewEvent(string[] args, string timezone);
        Event ParseUpdateEvent(Event evt, string[] args, string timezone);
        Event ParseUpdateEvent(Event evt, string args, string timezone);
    }
}