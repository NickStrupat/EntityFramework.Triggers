using System;
using System.Collections.Generic;
using System.Linq;

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
	internal class TriggerInvoker<TDbContext> : ITriggerInvoker where TDbContext : DbContext {

		public List<DelegateSynchronyUnion<DbContext>> RaiseChangingEvents(DbContext dbContext, IServiceProvider serviceProvider) {
			var entries = dbContext.ChangeTracker.Entries().ToList();
			var triggeredEntries = new List<EntityEntry>(entries.Count);
			var afterEvents = new List<DelegateSynchronyUnion<DbContext>>(entries.Count);
			while (entries.Any()) {
				foreach (var entry in entries) {
					var cancel = false;

					var after = RaiseChangingEvent(entry, dbContext, serviceProvider, ref cancel);
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

		private static EntityState GetCanceledEntityState(EntityState entityState) {
			switch (entityState) {
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

		private static DelegateSynchronyUnion<DbContext>? RaiseChangingEvent(EntityEntry entry, DbContext dbContext, IServiceProvider serviceProvider, ref Boolean cancel) {
			var tDbContext = (TDbContext)dbContext;
			var entityType = entry.Entity.GetType();
			var triggerEntityInvoker = GenericServiceCache<ITriggerEntityInvoker<TDbContext>, TriggerEntityInvoker<TDbContext, Object>>.GetOrAdd(dbContext.GetType(), entityType);
			switch (entry.State) {
				case EntityState.Added:
					triggerEntityInvoker.RaiseInserting(serviceProvider, entry.Entity, tDbContext, ref cancel);
					return new DelegateSynchronyUnion<DbContext>(context => triggerEntityInvoker.RaiseInserted(serviceProvider, entry.Entity, (TDbContext) context));
				case EntityState.Deleted:
					triggerEntityInvoker.RaiseDeleting(serviceProvider, entry.Entity, tDbContext, ref cancel);
					return new DelegateSynchronyUnion<DbContext>(context => triggerEntityInvoker.RaiseDeleted(serviceProvider, entry.Entity, (TDbContext) context));
				case EntityState.Modified:
					triggerEntityInvoker.RaiseUpdating(serviceProvider, entry.Entity, tDbContext, ref cancel);
					return new DelegateSynchronyUnion<DbContext>(context => triggerEntityInvoker.RaiseUpdated(serviceProvider, entry.Entity, (TDbContext) context));
			}
			return null;
		}

		public void RaiseChangedEvents(DbContext dbContext, IServiceProvider serviceProvider, IEnumerable<DelegateSynchronyUnion<DbContext>> afterEvents) {
			foreach (var after in afterEvents)
				after.Invoke(dbContext);
		}

		public Boolean RaiseFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, DbUpdateException dbUpdateException, ref Boolean swallow) {
			var context = (TDbContext) dbContext;

			IEnumerable<EntityEntry> entries;
			
			if (dbUpdateException.Entries.Any()) {
				entries = dbUpdateException.Entries;
			}
			else {
				entries = dbContext.ChangeTracker.Entries().ToArray();
				if (entries.Count() != 1) {
					swallow = false;
					return swallow;
				}
			}
			RaiseTheFailedEvents(context, serviceProvider, entries, dbUpdateException, ref swallow);
			return swallow;
		}

#if !EF_CORE
		public Boolean RaiseFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, DbEntityValidationException dbEntityValidationException, ref Boolean swallow) {
			var context = (TDbContext) dbContext;
			RaiseTheFailedEvents(context, serviceProvider, dbEntityValidationException.EntityValidationErrors.Select(x => x.Entry), dbEntityValidationException, ref swallow);
			return swallow;
		}
#endif

		public Boolean RaiseFailedEvents(DbContext dbContext, IServiceProvider serviceProvider, Exception exception, ref Boolean swallow) {
			var context = (TDbContext) dbContext;
			var entries = dbContext.ChangeTracker.Entries().ToArray();
			if (entries.Length != 1) {
				swallow = false;
				return swallow;
			}
			RaiseTheFailedEvents(context, serviceProvider, entries, exception, ref swallow);
			return swallow;
		}

		private static void RaiseTheFailedEvents(TDbContext dbContext, IServiceProvider serviceProvider, IEnumerable<EntityEntry> entries, Exception exception, ref Boolean swallow) {
			foreach (var entry in entries)
				RaiseTheFailedEvents(dbContext, serviceProvider, entry, exception, ref swallow);
		}

		private static void RaiseTheFailedEvents(TDbContext dbContext, IServiceProvider serviceProvider, EntityEntry entry, Exception exception, ref Boolean swallow) {
			switch (entry.State) {
				case EntityState.Added:
					GetTriggerEntityInvoker(entry.Entity.GetType()).RaiseInsertFailed(serviceProvider, entry.Entity, dbContext, exception, ref swallow);
					break;
				case EntityState.Modified:
					GetTriggerEntityInvoker(entry.Entity.GetType()).RaiseUpdateFailed(serviceProvider, entry.Entity, dbContext, exception, ref swallow);
					break;
				case EntityState.Deleted:
					GetTriggerEntityInvoker(entry.Entity.GetType()).RaiseDeleteFailed(serviceProvider, entry.Entity, dbContext, exception, ref swallow);
					break;
			}
			ITriggerEntityInvoker<TDbContext> GetTriggerEntityInvoker(Type entityType) =>
				GenericServiceCache<ITriggerEntityInvoker<TDbContext>, TriggerEntityInvoker<TDbContext, Object>>.GetOrAdd(dbContext.GetType(), entityType);
		}
	}
}