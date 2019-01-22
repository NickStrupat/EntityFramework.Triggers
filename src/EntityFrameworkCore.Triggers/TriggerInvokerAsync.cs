using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using EntityEntry = System.Data.Entity.Infrastructure.DbEntityEntry;
namespace EntityFramework.Triggers
#endif
{
	internal class TriggerInvokerAsync<TDbContext> : ITriggerInvokerAsync where TDbContext : DbContext
	{
		public async Task<List<DelegateSynchronyUnion<DbContext>>> RaiseChangingEventsAsync(DbContext dbContext, IServiceProvider serviceProvider)
		{
			var entries = dbContext.ChangeTracker.Entries().ToList();
			var triggeredEntries = new List<EntityEntry>(entries.Count);
			var afterEvents = new List<DelegateSynchronyUnion<DbContext>>(entries.Count);
			while (entries.Any())
			{
				foreach (var entry in entries)
				{
					var cancel = false;

					var after = await RaiseChangingEventAsync(entry, dbContext, serviceProvider, ref cancel);
					if (after != null && !cancel)
						afterEvents.Add(after.Value);

					triggeredEntries.Add(entry);
					if (cancel)
						entry.State = GetCanceledEntityState(entry.State);
				}
				var newEntries = dbContext.ChangeTracker.Entries().Except(triggeredEntries, EntityEntryComparer.Default);
				entries.Clear();
				entries.AddRange(newEntries);
			}
			return afterEvents;
		}

		private static EntityState GetCanceledEntityState(EntityState entityState)
		{
			switch (entityState)
			{
				case EntityState.Added:
					return EntityState.Detached;
				case EntityState.Deleted:
					return EntityState.Modified;
				case EntityState.Modified:
					return EntityState.Unchanged;
				default:
					return entityState;
			}
		}

		private static async Task<DelegateSynchronyUnion<DbContext>?> RaiseChangingEventAsync(EntityEntry entry, DbContext dbContext, IServiceProvider serviceProvider, ref Boolean cancel)
		{
			var tDbContext = (TDbContext)dbContext;
			var entityType = entry.Entity.GetType();
			var triggerEntityInvoker = TriggerEntityInvokers<TDbContext>.Get(entityType);
			switch (entry.State)
			{
				case EntityState.Added:
					await triggerEntityInvoker.RaiseInserting(serviceProvider, entry.Entity, tDbContext, ref cancel);
					return new DelegateSynchronyUnion<DbContext>(context => triggerEntityInvoker.RaiseInserted(serviceProvider, entry.Entity, (TDbContext)context));
				case EntityState.Deleted:
					await triggerEntityInvoker.RaiseDeleting(serviceProvider, entry.Entity, tDbContext, ref cancel);
					return new DelegateSynchronyUnion<DbContext>(context => triggerEntityInvoker.RaiseDeleted(serviceProvider, entry.Entity, (TDbContext)context));
				case EntityState.Modified:
					await triggerEntityInvoker.RaiseUpdating(serviceProvider, entry.Entity, tDbContext, ref cancel);
					return new DelegateSynchronyUnion<DbContext>(context => triggerEntityInvoker.RaiseUpdated(serviceProvider, entry.Entity, (TDbContext)context));
			}
			return null;
		}

		public void RaiseChangedEvents(DbContext dbContext, IServiceProvider serviceProvider, IEnumerable<List<Delegate>> afterEvents)
		{
			throw new NotImplementedException();
		}

		public Boolean RaiseFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, DbUpdateException dbUpdateException,
			ref Boolean swallow)
		{
			throw new NotImplementedException();
		}

		public Boolean RaiseFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, Exception exception,
			ref Boolean swallow)
		{
			throw new NotImplementedException();
		}
	}
}