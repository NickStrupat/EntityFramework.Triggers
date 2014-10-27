using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Triggers {
	public static class Extensions {
		public static Triggers<TTriggerable> Triggers<TTriggerable>(this TTriggerable triggerable)
			where TTriggerable : class, ITriggerable<TTriggerable>, new() {
			return TriggersWeak<TTriggerable>.ConditionalWeakTable.GetValue(triggerable, key => new Triggers<TTriggerable> { Triggerable = triggerable });
		}

		private static readonly MethodInfo triggersMethodInfo = typeof(Extensions).GetMethod("Triggers");
		private static readonly Dictionary<Type, MethodInfo> triggerGenericMethodInfoCache = new Dictionary<Type, MethodInfo>();
		private static MethodInfo getTriggerMethodInfo(Type entityType) {
			MethodInfo triggerGenericMethodInfo;
			if (!triggerGenericMethodInfoCache.TryGetValue(entityType, out triggerGenericMethodInfo)) {
				triggerGenericMethodInfo = triggersMethodInfo.MakeGenericMethod(entityType);
				triggerGenericMethodInfoCache.Add(entityType, triggerGenericMethodInfo);
			}
			return triggerGenericMethodInfo;
		}
		private static IEnumerable<Action<DbContext>> RaiseTheBeforeEvents(this DbContext dbContext) {
			var afterActions = new List<Action<DbContext>>();
			foreach (var entry in dbContext.ChangeTracker.Entries<ITriggerable>()) {
				var entityType = entry.Entity.GetType();
				var triggerGenericMethodInfo = getTriggerMethodInfo(entityType);
				var triggers = (ITriggers<DbContext>)triggerGenericMethodInfo.Invoke(null, new[] { entry.Entity });
				switch (entry.State) {
					case EntityState.Added:
						triggers.OnBeforeInsert(dbContext);
						if (entry.State == EntityState.Added)
							afterActions.Add(triggers.OnAfterInsert);
						break;
					case EntityState.Deleted:
						triggers.OnBeforeDelete(dbContext);
						if (entry.State == EntityState.Deleted)
							afterActions.Add(triggers.OnAfterDelete);
						break;
					case EntityState.Modified:
						triggers.OnBeforeUpdate(dbContext);
						if (entry.State == EntityState.Modified)
							afterActions.Add(triggers.OnAfterUpdate);
						break;
				}
			}
			return afterActions;
		}

		private static void RaiseTheAfterEvents(this DbContext dbContext, IEnumerable<Action<DbContext>> afterActions) {
			foreach (var afterAction in afterActions)
				afterAction(dbContext);
		}

		private static void RaiseTheFailedEvents(this DbContext dbContext, Exception exception) {
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

		private static void RaiseTheFailedEvents(this DbContext dbContext, DbEntityEntry entry, Exception exception) {
			var entityType = entry.Entity.GetType();
			var triggerGenericMethodInfo = getTriggerMethodInfo(entityType);
			var triggers = (ITriggers<DbContext>)triggerGenericMethodInfo.Invoke(null, new[] { entry.Entity });
			switch (entry.State) {
				case EntityState.Added:
					triggers.OnInsertFailed(dbContext, exception);
					break;
				case EntityState.Modified:
					triggers.OnUpdateFailed(dbContext, exception);
					break;
				case EntityState.Deleted:
					triggers.OnDeleteFailed(dbContext, exception);
					break;
			}
		}

		/// <summary>
		/// Save changes to the store, firing trigger events accordingly
		/// </summary>
		/// <param name="dbContext"></param>
		/// <returns></returns>
		public static Int32 SaveChangesWithTriggers(this DbContext dbContext, Func<Int32> baseSaveChanges) {
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
