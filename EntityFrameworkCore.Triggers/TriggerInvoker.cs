using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using EntityEntry = System.Data.Entity.Infrastructure.DbEntityEntry;
namespace EntityFramework.Triggers {
#endif
	internal class EntityEntryComparer : IEqualityComparer<EntityEntry> {
		private EntityEntryComparer() {}
		public Boolean Equals(EntityEntry x, EntityEntry y) => ReferenceEquals(x.Entity, y.Entity);
		public Int32 GetHashCode(EntityEntry obj) => obj.Entity.GetHashCode();
		public static readonly EntityEntryComparer Default = new EntityEntryComparer();
	}

	internal class TriggerInvoker<TDbContext> : ITriggerInvoker where TDbContext : DbContext {
		private static readonly Type BaseDbContextType = typeof(TDbContext).GetTypeInfo().BaseType;
		private static readonly Boolean IsADbContextType = typeof(DbContext).IsAssignableFrom(BaseDbContextType);
		private static readonly ITriggerInvoker BaseTriggerInvoker = IsADbContextType ? TriggerInvokers.Get(BaseDbContextType) : null;

		public List<Action<DbContext>> RaiseTheBeforeEvents(DbContext dbContext) {
			var entries = dbContext.ChangeTracker.Entries().ToList();
			var triggeredEntries = new List<EntityEntry>(entries.Count);
			var afterEvents = new List<Action<DbContext>>(entries.Count);
			while (entries.Any()) {
				foreach (var entry in entries) {
					var cancel = false;
					RaiseTheBeforeEventInner(dbContext, entry, triggeredEntries, afterEvents, ref cancel);
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


		public void RaiseTheBeforeEventInner(DbContext dbContext, EntityEntry entry, List<EntityEntry> triggeredEntries, List<Action<DbContext>> afterEvents, ref Boolean cancel) {
			BaseTriggerInvoker?.RaiseTheBeforeEventInner(dbContext, entry, triggeredEntries, afterEvents, ref cancel);
			var after = RaiseTheBeforeEvent(entry, dbContext, ref cancel);
			triggeredEntries.Add(entry);
			if (after != null && !cancel)
				afterEvents.Add(after);
		}

		private static Action<DbContext> RaiseTheBeforeEvent(EntityEntry entry, DbContext dbContext, ref Boolean cancel) {
			var tDbContext = (TDbContext)dbContext;
			var triggers = TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType());
			switch (entry.State) {
				case EntityState.Added:
					triggers.RaiseInserting(entry.Entity, tDbContext, ref cancel);
					return context => triggers.RaiseInserted(entry.Entity, (TDbContext) context);
				case EntityState.Deleted:
					triggers.RaiseDeleting(entry.Entity, tDbContext, ref cancel);
					return context => triggers.RaiseDeleted(entry.Entity, (TDbContext) context);
				case EntityState.Modified:
					triggers.RaiseUpdating(entry.Entity, tDbContext, ref cancel);
					return context => triggers.RaiseUpdated(entry.Entity, (TDbContext) context);
			}
			return null;
		}

		public void RaiseTheAfterEvents(DbContext dbContext, IEnumerable<Action<DbContext>> afterEvents) {
			foreach (var after in afterEvents)
				after(dbContext);
		}

		public void RaiseTheFailedEvents(DbContext dbContext, DbUpdateException dbUpdateException, ref Boolean swallow) {
			BaseTriggerInvoker?.RaiseTheFailedEvents(dbContext, dbUpdateException, ref swallow);
			var context = (TDbContext) dbContext;

			IEnumerable<EntityEntry> entries;
			
			if (dbUpdateException.Entries.Any()) {
				entries = dbUpdateException.Entries;
			}
			else {
				entries = dbContext.ChangeTracker.Entries().ToArray();
				if (entries.Count() != 1) {
					swallow = false;
					return;
				}
			}
			RaiseTheFailedEvents(context, entries, dbUpdateException, ref swallow);

		}

#if !EF_CORE
		public void RaiseTheFailedEvents(DbContext dbContext, DbEntityValidationException dbEntityValidationException, ref Boolean swallow) {
			BaseTriggerInvoker?.RaiseTheFailedEvents(dbContext, dbEntityValidationException, ref swallow);
			var context = (TDbContext) dbContext;
			RaiseTheFailedEvents(context, dbEntityValidationException.EntityValidationErrors.Select(x => x.Entry), dbEntityValidationException, ref swallow);
		}
#endif

		public void RaiseTheFailedEvents(DbContext dbContext, Exception exception, ref Boolean swallow) {
			BaseTriggerInvoker?.RaiseTheFailedEvents(dbContext, exception, ref swallow);
			var context = (TDbContext) dbContext;
			var entries = dbContext.ChangeTracker.Entries().ToArray();
			if (entries.Length != 1) {
				swallow = false;
				return;
			}
			RaiseTheFailedEvents(context, entries, exception, ref swallow);
		}

		private static void RaiseTheFailedEvents(TDbContext dbContext, IEnumerable<EntityEntry> entries, Exception exception, ref Boolean swallow) {
			foreach (var entry in entries)
				RaiseTheFailedEvents(dbContext, entry, exception, ref swallow);
		}

		private static void RaiseTheFailedEvents(TDbContext dbContext, EntityEntry entry, Exception exception, ref Boolean swallow) {
			switch (entry.State) {
				case EntityState.Added:
					TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType()).RaiseInsertFailed(entry.Entity, dbContext, exception, ref swallow);
					break;
				case EntityState.Modified:
					TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType()).RaiseUpdateFailed(entry.Entity, dbContext, exception, ref swallow);
					break;
				case EntityState.Deleted:
					TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType()).RaiseDeleteFailed(entry.Entity, dbContext, exception, ref swallow);
					break;
			}
		}
	}
}