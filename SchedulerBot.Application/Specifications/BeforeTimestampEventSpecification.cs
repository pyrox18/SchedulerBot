using SchedulerBot.Data.Models;
using System;
using System.Linq.Expressions;

namespace SchedulerBot.Application.Specifications
{
    public class BeforeTimestampEventSpecification : BaseSpecification<Event>
    {
        public BeforeTimestampEventSpecification(DateTimeOffset timestamp)
            : base(x => x.EndTimestamp <= timestamp)
        {
        }
    }
}
