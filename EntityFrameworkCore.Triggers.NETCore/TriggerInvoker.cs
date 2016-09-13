using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityFrameworkCore.Triggers {
	internal class TriggerInvoker<TDbContext> : ITriggerInvoker where TDbContext : DbContext {
		private static readonly Type BaseDbContextType = typeof(TDbContext).GetTypeInfo().BaseType;
		private static readonly Boolean IsADbContextType = typeof(DbContext).IsAssignableFrom(BaseDbContextType);
		private static readonly ITriggerInvoker BaseTriggerInvoker = IsADbContextType ? TriggerInvokers.Get(BaseDbContextType) : null;

		public List<Action<DbContext>> RaiseTheBeforeEvents(DbContext dbContext) {
			var theAfterEvents = BaseTriggerInvoker?.RaiseTheBeforeEvents(dbContext) ?? new List<Action<DbContext>>();

			var entries = dbContext.ChangeTracker.Entries();
			var context = (TDbContext) dbContext;
			foreach (var entry in entries) {
				var after = RaiseTheBeforeEvent(entry, context);
				if (after != null)
					theAfterEvents.Add((Action<DbContext>) after);
			}
			return theAfterEvents;
		}

		private Action<TDbContext> RaiseTheBeforeEvent(EntityEntry entry, TDbContext dbContext) {
			var triggers = TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType());
			switch (entry.State) {
				case EntityState.Added:
					triggers.RaiseBeforeInsert(entry.Entity, dbContext);
					if (entry.State == EntityState.Added)
						return context => triggers.RaiseAfterInsert(entry.Entity, context);
					break;
				case EntityState.Deleted:
					triggers.RaiseBeforeDelete(entry.Entity, dbContext);
					if (entry.State == EntityState.Deleted)
						return context => triggers.RaiseAfterDelete(entry.Entity, context);
					break;
				case EntityState.Modified:
					triggers.RaiseBeforeUpdate(entry.Entity, dbContext);
					if (entry.State == EntityState.Modified)
						return context => triggers.RaiseAfterUpdate(entry.Entity, context);
					break;
			}
			return null;
		}

		public void RaiseTheAfterEvents(DbContext dbContext, IEnumerable<Action<DbContext>> afterEvents) {
			foreach (var after in afterEvents)
				after(dbContext);
		}

		public Int32 BaseSaveChanges(DbContext dbContext, Boolean acceptAllChangesOnSuccess) {
			return ParentDbContext<TDbContext>.SaveChanges((TDbContext) dbContext, acceptAllChangesOnSuccess); // must call as TDbContext
		}

		public Task<Int32> BaseSaveChangesAsync(DbContext dbContext, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken) {
			return ParentDbContext<TDbContext>.SaveChangesAsync((TDbContext) dbContext, acceptAllChangesOnSuccess, cancellationToken); // must call as TDbContext
		}

		public void RaiseTheFailedEvents(DbContext dbContext, DbUpdateException dbUpdateException) {
			var context = (TDbContext) dbContext;
			foreach (var entry in dbUpdateException.Entries)
				RaiseTheFailedEvents(context, entry, dbUpdateException);
			foreach (var entry in dbUpdateException.Entries)
				RaiseTheFailedEvents(context, entry, dbUpdateException);
		}

		private static void RaiseTheFailedEvents(TDbContext dbContext, EntityEntry entry, Exception exception) {
			var triggers = TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType());
			switch (entry.State) {
				case EntityState.Added:
					triggers.RaiseInsertFailed(entry.Entity, dbContext, exception);
					break;
				case EntityState.Modified:
					triggers.RaiseUpdateFailed(entry.Entity, dbContext, exception);
					break;
				case EntityState.Deleted:
					triggers.RaiseDeleteFailed(entry.Entity, dbContext, exception);
					break;
			}
		}
	}
}