using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerBot.Data.Services
{
    public class CalendarService : ICalendarService
    {
        private readonly SchedulerBotContext _db;

        public CalendarService(SchedulerBotContext context) => _db = context;
    }
}
