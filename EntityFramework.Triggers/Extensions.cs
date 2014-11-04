using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Triggers {
	public static class Extensions {
		internal static ITriggers<TDbContext> Triggers<TDbContext>(this ITriggerable triggerable)
			where TDbContext : DbContext
		{
			ITriggers<TDbContext> triggers;
			TriggersWeakRefs<TDbContext>.ConditionalWeakTable.TryGetValue(triggerable, out triggers);
			return triggers;
		}

		public static Triggers<TTriggerable, TDbContext> Triggers<TTriggerable, TDbContext>(this TTriggerable triggerable)
			where TTriggerable : class, ITriggerable
			where TDbContext : DbContext
		{
			return (Triggers<TTriggerable, TDbContext>) TriggersWeakRefs<TDbContext>.ConditionalWeakTable.GetValue(triggerable, key => new Triggers<TTriggerable, TDbContext>());
		}

		private static IEnumerable<Action<TDbContext>> RaiseTheBeforeEvents<TDbContext>(this TDbContext dbContext)
			where TDbContext : DbContext
		{
			var afterActions = new List<Action<TDbContext>>();
			foreach (var entry in dbContext.ChangeTracker.Entries<ITriggerable>()) {
				var triggers = entry.Entity.Triggers<TDbContext>();
				if (triggers == null)
					continue;
				switch (entry.State) {
					case EntityState.Added:
						triggers.OnBeforeInsert(entry.Entity, dbContext);
						if (entry.State == EntityState.Added)
							afterActions.Add(context => triggers.OnAfterInsert(entry.Entity, context));
						break;
					case EntityState.Deleted:
						triggers.OnBeforeDelete(entry.Entity, dbContext);
						if (entry.State == EntityState.Deleted)
							afterActions.Add(context => triggers.OnAfterDelete(entry.Entity, context));
						break;
					case EntityState.Modified:
						triggers.OnBeforeUpdate(entry.Entity, dbContext);
						if (entry.State == EntityState.Modified)
							afterActions.Add(context => triggers.OnAfterUpdate(entry.Entity, context));
						break;
				}
			}
			return afterActions;
		}

		private static void RaiseTheAfterEvents<TDbContext>(this TDbContext dbContext, IEnumerable<Action<TDbContext>> afterActions) where TDbContext : DbContext {
			foreach (var afterAction in afterActions)
				afterAction(dbContext);
		}

		private static void RaiseTheFailedEvents<TDbContext>(this TDbContext dbContext, Exception exception) where TDbContext : DbContext {
			var dbUpdateException = exception as DbUpdateException;
			var dbEntityValidationException = exception as DbEntityValidationException;
			if (dbUpdateException != null) {
				foreach (var entry in dbUpdateException.Entries.Where(x => x.Entity is ITriggerable))
					RaiseTheFailedEvents(dbContext, entry, dbUpdateException);
			}
			else if (dbEntityValidationException != null) {
				foreach (var dbEntityValidationResult in dbEntityValidationException.EntityValidationErrors.Where(x => x.Entry.Entity is ITriggerable))
					RaiseTheFailedEvents(dbContext, dbEntityValidationResult.Entry, dbEntityValidationException);
			}
		}

		private static void RaiseTheFailedEvents<TDbContext>(this TDbContext dbContext, DbEntityEntry entry, Exception exception) where TDbContext : DbContext {
			var triggerable = (ITriggerable) entry.Entity;
			var triggers = triggerable.Triggers<TDbContext>();
			if (triggers == null)
				return;
			switch (entry.State) {
				case EntityState.Added:
					triggers.OnInsertFailed(triggerable, dbContext, exception);
					break;
				case EntityState.Modified:
					triggers.OnUpdateFailed(triggerable, dbContext, exception);
					break;
				case EntityState.Deleted:
					triggers.OnDeleteFailed(triggerable, dbContext, exception);
					break;
			}
		}

		/// <summary>
		/// Save changes to the store, firing trigger events accordingly
		/// </summary>
		/// <param name="dbContext"></param>
		/// <returns></returns>
		public static Int32 SaveChangesWithTriggers<TDbContext>(this TDbContext dbContext, Func<Int32> baseSaveChanges) where TDbContext : DbContext {
			try {
				using (new InstanceReEntrancyGuard(dbContext, "When overriding SaveChanges(), you must pass `base.SaveChanges` to SaveChangesWithTriggers(). For example, { public override Int32 SaveChanges() { this.SaveChangesWithTriggers(base.SaveChanges); } }", () => baseSaveChanges == null)) {
					var afterActions = dbContext.RaiseTheBeforeEvents();
					var result = baseSaveChanges();
					dbContext.RaiseTheAfterEvents(afterActions);
					return result;
				}
			}
			catch (Exception exception) {
				dbContext.RaiseTheFailedEvents(exception);
				throw;
			}
		}

		public static Task<Int32> SaveChangesWithTriggersAsync<TDbContext>(this TDbContext dbContext, Func<CancellationToken, Task<Int32>> baseSaveChangesAsync) where TDbContext : DbContext {
			return dbContext.SaveChangesWithTriggersAsync(baseSaveChangesAsync, CancellationToken.None);
		}

		public static async Task<Int32> SaveChangesWithTriggersAsync<TDbContext>(this TDbContext dbContext, Func<CancellationToken, Task<Int32>> baseSaveChangesAsync, CancellationToken cancellationToken) where TDbContext : DbContext {
			try {
				using (new InstanceReEntrancyGuard(dbContext, "When overriding SaveChangesAsync(), you must pass `base.SaveChangesAsync` to SaveChangesWithTriggersAsync(). For example, { public override Int32 SaveChangesAsync() { this.SaveChangesWithTriggers(base.SaveChangesAsync); } }", () => baseSaveChangesAsync == null)) {
					var afterActions = dbContext.RaiseTheBeforeEvents();
					var result = await baseSaveChangesAsync(cancellationToken);
					dbContext.RaiseTheAfterEvents(afterActions);
					return result;
				}
			}
			catch (Exception exception) {
				dbContext.RaiseTheFailedEvents(exception);
				throw;
			}
		}
	}
}
