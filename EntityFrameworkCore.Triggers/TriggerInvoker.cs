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

		public List<Action<DbContext>> RaiseTheBeforeEvents(DbContext dbContext) {
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

		public Boolean RaiseTheFailedEvents(DbContext dbContext, DbUpdateException dbUpdateException, Boolean swallow) {
			if (BaseTriggerInvoker != null)
				swallow = BaseTriggerInvoker.RaiseTheFailedEvents(dbContext, dbUpdateException, swallow);
			var context = (TDbContext) dbContext;

			IEnumerable<EntityEntry> entries;
			
			if (dbUpdateException.Entries.Any()) {
				entries = dbUpdateException.Entries;
			}
			else {
				entries = dbContext.ChangeTracker.Entries().ToArray();
				if (entries.Count() != 1)
					return false;
			}
			return RaiseTheFailedEvents(context, entries, dbUpdateException, swallow);

		}

#if !EF_CORE
		public Boolean RaiseTheFailedEvents(DbContext dbContext, DbEntityValidationException dbEntityValidationException, Boolean swallow) {
			if (BaseTriggerInvoker != null)
				swallow = BaseTriggerInvoker.RaiseTheFailedEvents(dbContext, dbEntityValidationException, swallow);
			var context = (TDbContext) dbContext;
			return RaiseTheFailedEvents(context, dbEntityValidationException.EntityValidationErrors.Select(x => x.Entry), dbEntityValidationException, swallow);
		}
#endif

		public Boolean RaiseTheFailedEvents(DbContext dbContext, Exception exception, Boolean swallow) {
			if (BaseTriggerInvoker != null)
				swallow = BaseTriggerInvoker.RaiseTheFailedEvents(dbContext, exception, swallow);
			var context = (TDbContext) dbContext;
			var entries = dbContext.ChangeTracker.Entries().ToArray();
			if (entries.Length != 1)
				return false;
			return RaiseTheFailedEvents(context, entries, exception, swallow);
		}

		private static Boolean RaiseTheFailedEvents(TDbContext dbContext, IEnumerable<EntityEntry> entries, Exception exception, Boolean swallow) {
			foreach (var entry in entries)
				swallow = RaiseTheFailedEvents(dbContext, entry, exception, swallow);
			return swallow;
		}

		private static Boolean RaiseTheFailedEvents(TDbContext dbContext, EntityEntry entry, Exception exception, Boolean swallow) {
			switch (entry.State) {
				case EntityState.Added:
					return TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType()).RaiseInsertFailed(entry.Entity, dbContext, exception, swallow);
				case EntityState.Modified:
					return TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType()).RaiseUpdateFailed(entry.Entity, dbContext, exception, swallow);
				case EntityState.Deleted:
					return TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType()).RaiseDeleteFailed(entry.Entity, dbContext, exception, swallow);
				default:
					return swallow;
			}
		}
	}
}