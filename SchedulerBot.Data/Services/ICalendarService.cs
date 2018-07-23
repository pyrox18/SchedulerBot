using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SchedulerBot.Data.Models;

namespace SchedulerBot.Data.Services
{
    public interface ICalendarService
    {
        Task<Calendar> CreateCalendarAsync(Calendar calendar);
        Task<bool> DeleteCalendarAsync(ulong calendarId);
    }
}
