using System;
using System.Collections.Generic;
using System.Text;

namespace SchedulerBot.Data.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly SchedulerBotContext _db;

        public PermissionService(SchedulerBotContext context) => _db = context;
    }
}
