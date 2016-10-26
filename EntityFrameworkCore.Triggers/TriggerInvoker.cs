using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

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

		public List<Action<DbContext>> RaiseTheBeforeEvents(DbContext dbContext) {
			var theAfterEvents = BaseTriggerInvoker?.RaiseTheBeforeEvents(dbContext) ?? new List<Action<DbContext>>();

			var entries = dbContext.ChangeTracker.Entries();
#if EF_CORE
			entries = entries.ToArray();
#endif
			foreach (var entry in entries) {
				var after = RaiseTheBeforeEvent(entry, dbContext);
				if (after != null)
					theAfterEvents.Add(after);
			}
			return theAfterEvents;
		}

		private Action<DbContext> RaiseTheBeforeEvent(EntityEntry entry, DbContext dbContext) {
			var tDbContext = (TDbContext)dbContext;
			var triggers = TriggerEntityInvokers<TDbContext>.Get(entry.Entity.GetType());
			switch (entry.State) {
				case EntityState.Added:
					triggers.RaiseBeforeInsert(entry.Entity, tDbContext);
					if (entry.State == EntityState.Added)
						return context => triggers.RaiseAfterInsert(entry.Entity, tDbContext);
					break;
				case EntityState.Deleted:
					triggers.RaiseBeforeDelete(entry.Entity, tDbContext);
					if (entry.State == EntityState.Deleted)
						return context => triggers.RaiseAfterDelete(entry.Entity, tDbContext);
					break;
				case EntityState.Modified:
					triggers.RaiseBeforeUpdate(entry.Entity, tDbContext);
					if (entry.State == EntityState.Modified)
						return context => triggers.RaiseAfterUpdate(entry.Entity, tDbContext);
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