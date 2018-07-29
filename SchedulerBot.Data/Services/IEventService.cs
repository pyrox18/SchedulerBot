using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Data.Services
{
    public interface IEventService
    {
        Task<Event> CreateEventAsync(ulong calendarId, Event evt);
        Task<List<Event>> GetEventsAsync(ulong calendarId);
        Task<Event> DeleteEventAsync(ulong calendarId, int index);
    }
}
