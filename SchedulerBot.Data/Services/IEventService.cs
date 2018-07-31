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
        Task<Event> DeleteEventAsync(Guid eventId);
        Task<Event> GetEventByIndexAsync(ulong calendarId, int index);
        Task<Event> UpdateEventAsync(Event evt);
        Task<Event> ApplyRepeatAsync(Guid eventId);
        Task<List<Event>> GetEventsInHourIntervalAsync(double hours);
    }
}
