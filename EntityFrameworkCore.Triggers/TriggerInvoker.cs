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
	internal class TriggerInvoker<TDbContext> : ITriggerInvoker where TDbContext : DbContext {
		private static readonly Type BaseDbContextType = typeof(TDbContext).GetTypeInfo().BaseType;
		private static readonly Boolean IsADbContextType = typeof(DbContext).IsAssignableFrom(BaseDbContextType);
		private static readonly ITriggerInvoker BaseTriggerInvoker = IsADbContextType ? TriggerInvokers.Get(BaseDbContextType) : null;

		private class EntityEntryComparer : IEqualityComparer<EntityEntry> {
			private EntityEntryComparer() {}
			public Boolean Equals(EntityEntry x, EntityEntry y) => ReferenceEquals(x.Entity, y.Entity);
			public Int32 GetHashCode(EntityEntry obj) => obj.Entity.GetHashCode();
			public static readonly EntityEntryComparer Default = new EntityEntryComparer();
		}

		public List<Action<DbContext>> RaiseTheBeforeEventsOuter(DbContext dbContext) {
			var entries = dbContext.ChangeTracker.Entries().ToList();
			var triggeredEntries = new List<EntityEntry>();
			var afterEvents = new List<Action<DbContext>>();
			while (entries.Any()) {
				RaiseTheBeforeEventsInner(dbContext, entries, triggeredEntries, afterEvents);
				var newEntries = dbContext.ChangeTracker.Entries().Except(triggeredEntries, EntityEntryComparer.Default);
				entries.Clear();
				entries.AddRange(newEntries);
			}
			return afterEvents;
		}

		public void RaiseTheBeforeEventsInner(DbContext dbContext, List<EntityEntry> entries, List<EntityEntry> triggeredEntries, List<Action<DbContext>> afterEvents) {
			BaseTriggerInvoker?.RaiseTheBeforeEventsInner(dbContext, entries, triggeredEntries, afterEvents);
			foreach (var entry in entries) {
				var after = RaiseTheBeforeEvent(entry, dbContext);
				triggeredEntries.Add(entry);
				if (after != null)
					afterEvents.Add(after);
			}
		}

		private Action<DbContext> RaiseTheBeforeEvent(EntityEntry entry, DbContext dbContext) {
			var tDbContext = (TDbContext)dbContext;
			var triggers = TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType());
			switch (entry.State) {
				case EntityState.Added:
					triggers.RaiseBeforeInsert(entry.Entity, tDbContext);
					if (entry.State == EntityState.Added)
						return context => triggers.RaiseAfterInsert(entry.Entity, (TDbContext) context);
					break;
				case EntityState.Deleted:
					triggers.RaiseBeforeDelete(entry.Entity, tDbContext);
					if (entry.State == EntityState.Deleted)
						return context => triggers.RaiseAfterDelete(entry.Entity, (TDbContext) context);
					break;
				case EntityState.Modified:
					triggers.RaiseBeforeUpdate(entry.Entity, tDbContext);
					if (entry.State == EntityState.Modified)
						return context => triggers.RaiseAfterUpdate(entry.Entity, (TDbContext) context);
					break;
			}
			return null;
		}

		public void RaiseTheAfterEvents(DbContext dbContext, IEnumerable<Action<DbContext>> afterEvents) {
			foreach (var after in afterEvents)
				after(dbContext);
		}

		public void RaiseTheFailedEvents(DbContext dbContext, DbUpdateException dbUpdateException) {
			var context = (TDbContext) dbContext;
			foreach (var entry in dbUpdateException.Entries)
				RaiseTheFailedEvents(context, entry, dbUpdateException);
		}

#if !EF_CORE
		public void RaiseTheFailedEvents(DbContext dbContext, DbEntityValidationException dbEntityValidationException) {
			var context = (TDbContext) dbContext;
			foreach (var entry in dbEntityValidationException.EntityValidationErrors)
				RaiseTheFailedEvents(context, entry.Entry, dbEntityValidationException);
		}
#endif

		private static void RaiseTheFailedEvents(TDbContext dbContext, EntityEntry entry, Exception exception) {
			switch (entry.State) {
				case EntityState.Added:
					TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType()).RaiseInsertFailed(entry.Entity, dbContext, exception);
					break;
				case EntityState.Modified:
					TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType()).RaiseUpdateFailed(entry.Entity, dbContext, exception);
					break;
				case EntityState.Deleted:
					TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType()).RaiseDeleteFailed(entry.Entity, dbContext, exception);
					break;
			}
		}
	}
}