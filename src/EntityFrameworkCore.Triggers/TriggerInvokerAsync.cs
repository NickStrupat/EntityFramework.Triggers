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
					DelegateSynchronyUnion<DbContext>? after;
					(after, cancel) = await RaiseChangingEventAsync(entry, dbContext, serviceProvider, cancel);
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

		private static async Task<(DelegateSynchronyUnion<DbContext>?, Boolean)> RaiseChangingEventAsync(EntityEntry entry, DbContext dbContext, IServiceProvider serviceProvider, Boolean cancel)
		{
			var tDbContext = (TDbContext)dbContext;
			var entityType = entry.Entity.GetType();
			var triggerEntityInvoker = GenericServiceCache<ITriggerEntityInvoker<TDbContext>, TriggerEntityInvoker<TDbContext, Object>>.GetOrAdd(tDbContext.GetType(), entityType);
			switch (entry.State)
			{
				case EntityState.Added:
					cancel = await triggerEntityInvoker.RaiseInsertingAsync(serviceProvider, entry.Entity, tDbContext, cancel);
					return (new DelegateSynchronyUnion<DbContext>(context => triggerEntityInvoker.RaiseInsertedAsync(serviceProvider, entry.Entity, (TDbContext)context)), cancel);
				case EntityState.Deleted:
					cancel = await triggerEntityInvoker.RaiseDeletingAsync(serviceProvider, entry.Entity, tDbContext, cancel);
					return (new DelegateSynchronyUnion<DbContext>(context => triggerEntityInvoker.RaiseDeletedAsync(serviceProvider, entry.Entity, (TDbContext)context)), cancel);
				case EntityState.Modified:
					cancel = await triggerEntityInvoker.RaiseUpdatingAsync(serviceProvider, entry.Entity, tDbContext, cancel);
					return (new DelegateSynchronyUnion<DbContext>(context => triggerEntityInvoker.RaiseUpdatedAsync(serviceProvider, entry.Entity, (TDbContext)context)), cancel);
			}
			return default;
		}

		public async Task RaiseChangedEventsAsync(DbContext dbContext, IServiceProvider serviceProvider, IEnumerable<DelegateSynchronyUnion<DbContext>> afterEvents)
		{
			foreach (var after in afterEvents)
				await after.InvokeAsync(dbContext);
		}

		public async Task<Boolean> RaiseFailedEventsAsync(DbContext dbContext, IServiceProvider serviceProvider, DbUpdateException dbUpdateException)
		{
			var context = (TDbContext)dbContext;

			IEnumerable<EntityEntry> entries;

			if (dbUpdateException.Entries.Any())
			{
				entries = dbUpdateException.Entries;
			}
			else
			{
				entries = dbContext.ChangeTracker.Entries().ToArray();
				if (entries.Count() != 1)
				{
					return false;
				}
			}
			return await RaiseFailedEventsInternalAsync(context, serviceProvider, entries, dbUpdateException);
		}

#if !EF_CORE
		public async Task<Boolean> RaiseFailedEventsAsync(DbContext dbContext, IServiceProvider serviceProvider, DbEntityValidationException dbEntityValidationException) {
			var context = (TDbContext) dbContext;
			return await RaiseFailedEventsInternalAsync(context, serviceProvider, dbEntityValidationException.EntityValidationErrors.Select(x => x.Entry), dbEntityValidationException);
		}
#endif

		public async Task<Boolean> RaiseFailedEventsAsync(DbContext dbContext, IServiceProvider serviceProvider, Exception exception)
		{
			var context = (TDbContext)dbContext;
			var entries = dbContext.ChangeTracker.Entries().ToArray();
			if (entries.Length != 1)
			{
				return false;
			}
			return await RaiseFailedEventsInternalAsync(context, serviceProvider, entries, exception);
		}

		private static async Task<Boolean> RaiseFailedEventsInternalAsync(TDbContext dbContext, IServiceProvider serviceProvider, IEnumerable<EntityEntry> entries, Exception exception)
		{
			var swallow = false;
			foreach (var entry in entries)
				swallow = await RaiseFailedEventsInternalAsync(dbContext, serviceProvider, entry, exception, swallow);
			return swallow;
		}

		private static async Task<Boolean> RaiseFailedEventsInternalAsync(TDbContext dbContext, IServiceProvider serviceProvider, EntityEntry entry, Exception exception, Boolean swallow)
		{
			switch (entry.State)
			{
				case EntityState.Added:
					return await GetTriggerEntityInvoker(entry.Entity.GetType()).RaiseInsertFailedAsync(serviceProvider, entry.Entity, dbContext, exception, swallow);
				case EntityState.Modified:
					return await GetTriggerEntityInvoker(entry.Entity.GetType()).RaiseUpdateFailedAsync(serviceProvider, entry.Entity, dbContext, exception, swallow);
				case EntityState.Deleted:
					return await GetTriggerEntityInvoker(entry.Entity.GetType()).RaiseDeleteFailedAsync(serviceProvider, entry.Entity, dbContext, exception, swallow);
				default:
					return default;
			}

			ITriggerEntityInvoker<TDbContext> GetTriggerEntityInvoker(Type entityType) =>
				GenericServiceCache<ITriggerEntityInvoker<TDbContext>, TriggerEntityInvoker<TDbContext, Object>>.GetOrAdd(dbContext.GetType(), entityType);
		}
	}
}