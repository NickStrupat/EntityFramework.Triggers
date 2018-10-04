using System;
using System.Collections.Generic;
using System.Linq;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
    public sealed class TriggersEqualityComparer<TEntity, TDbContext> : IEqualityComparer<ITriggers<TEntity, TDbContext>>
    where TEntity : class
    where TDbContext : DbContext
    {
        public static readonly TriggersEqualityComparer<TEntity, TDbContext> Instance = new TriggersEqualityComparer<TEntity, TDbContext>();

        public bool Equals(ITriggers<TEntity, TDbContext> x, ITriggers<TEntity, TDbContext> y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
                return false;

            (ITriggerEvent<TEntity, TDbContext> tex, ITriggerEvent<TEntity, TDbContext> tey) ResultSelector(ITriggerEvent<TEntity, TDbContext> tex, ITriggerEvent<TEntity, TDbContext> tey) => (tex, tey);

            foreach (var tes in GetTriggerEvents(x).Zip(GetTriggerEvents(y), ResultSelector))
                if (!ReferenceEquals(tes.tex, tes.tey))
                    return false;
            return true;
        }

        public int GetHashCode(ITriggers<TEntity, TDbContext> triggers)
        {
            var hashCode = 0x51ed270b;
            foreach (var triggerEvent in GetTriggerEvents(triggers))
                if (triggerEvent != null)
                    hashCode = (hashCode * -1521134295) + triggerEvent.GetHashCode();
            return hashCode;
        }

        private static IEnumerable<ITriggerEvent<TEntity, TDbContext>> GetTriggerEvents(ITriggers<TEntity, TDbContext> triggers) =>
            eventGetters.Select(x => x.Invoke(triggers));

        private delegate ITriggerEvent<TEntity, TDbContext> EventDelegate(ITriggers<TEntity, TDbContext> triggers);

        private static readonly EventDelegate[] eventGetters =
            typeof(ITriggers<TEntity, TDbContext>).GetProperties()
                                                  .Where(x => typeof(ITriggerEvent<TEntity, TDbContext>).IsAssignableFrom(x.PropertyType))
                                                  .OrderBy(x => x.Name, StringComparer.Ordinal)
                                                  .Select(x => (EventDelegate)x.GetGetMethod().CreateDelegate(typeof(EventDelegate)))
                                                  .ToArray();
    }
}