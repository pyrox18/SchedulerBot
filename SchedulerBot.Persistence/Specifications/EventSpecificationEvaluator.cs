using Microsoft.EntityFrameworkCore;
using SchedulerBot.Application.Interfaces;
using SchedulerBot.Data.Models;
using System.Linq;

namespace SchedulerBot.Persistence.Specifications
{
    public class EventSpecificationEvaluator
    {
        public static IQueryable<Event> GetQuery(IQueryable<Event> inputQuery, ISpecification<Event> specification)
        {
            var query = inputQuery;

            if (!(specification.Criteria is null))
            {
                query = query.Where(specification.Criteria);
            }

            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

            query = specification.IncludeStrings.Aggregate(query, (current, include) => current.Include(include));

            if (!(specification.OrderBy is null))
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (!(specification.OrderByDescending is null))
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            if (specification.IsPagingEnabled)
            {
                query = query.Skip(specification.Skip)
                    .Take(specification.Take);
            }

            return query;
        }
    }
}