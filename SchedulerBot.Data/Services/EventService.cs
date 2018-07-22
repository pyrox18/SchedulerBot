using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerBot.Data.Services
{
    public class EventService : IEventService
    {
        private readonly SchedulerBotContext _db;

        public EventService(SchedulerBotContext context) => _db = context;
    }
}
