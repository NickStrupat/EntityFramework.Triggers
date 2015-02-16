using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFramework.Triggers {
	public static class Extensions {
		private static readonly ConditionalWeakTable<ITriggerable, List<ITriggers>> TriggersWeakRefs = new ConditionalWeakTable<ITriggerable, List<ITriggers>>();
		private static List<ITriggers> Triggers(this ITriggerable triggerable) {
			List<ITriggers> triggers;
			TriggersWeakRefs.TryGetValue(triggerable, out triggers);
			return triggers;
		}

		/// <summary>
		/// Retrieve the <see cref="T:Triggers`1{TTriggerable}"/> object that contains the trigger events for this <see cref="ITriggerable"/>
		/// </summary>
		/// <typeparam name="TTriggerable"></typeparam>
		/// <param name="triggerable"></param>
		/// <returns></returns>
		public static Triggers<TTriggerable> Triggers<TTriggerable>(this TTriggerable triggerable) where TTriggerable : class, ITriggerable {
			var triggersList = TriggersWeakRefs.GetValue(triggerable, key => new List<ITriggers>());
			var triggers = triggersList.SingleOrDefault(x => x is Triggers<TTriggerable>);
			if (triggers == null) {
				triggers = new Triggers<TTriggerable>();
				triggersList.Add(triggers);
			}
			return (Triggers<TTriggerable>) triggers;
		}

		private static IEnumerable<Action<DbContext>> RaiseTheBeforeEvents(this DbContext dbContext) {
			var afterActions = new List<Action<DbContext>>();
			foreach (var entry in dbContext.ChangeTracker.Entries<ITriggerable>()) {
				var triggersList = entry.Entity.Triggers();
				if (triggersList == null)
					continue;
				foreach (var triggers in triggersList) {
					var entry1 = entry;
					var triggers1 = triggers;
					switch (entry.State) {
						case EntityState.Added:
							triggers.OnBeforeInsert(entry.Entity, dbContext);
							if (entry.State == EntityState.Added)
								afterActions.Add(context => triggers1.OnAfterInsert(entry1.Entity, context));
							break;
						case EntityState.Deleted:
							triggers.OnBeforeDelete(entry.Entity, dbContext);
							if (entry.State == EntityState.Deleted)
								afterActions.Add(context => triggers1.OnAfterDelete(entry1.Entity, context));
							break;
						case EntityState.Modified:
							triggers.OnBeforeUpdate(entry.Entity, dbContext);
							if (entry.State == EntityState.Modified)
								afterActions.Add(context => triggers1.OnAfterUpdate(entry1.Entity, context));
							break;
					}
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
			var triggerable = (ITriggerable) entry.Entity;
			var triggersList = triggerable.Triggers();
			if (triggersList == null)
				return;
			foreach (var triggers in triggersList) {
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
		}
		/// <summary>
		/// Saves all changes made in this context to the underlying database, firing trigger events accordingly.
		/// </summary>
		/// <param name="dbContext"></param>
		/// <param name="baseSaveChanges">A delegate to base.SaveChanges(). Always pass `base.SaveChanges`.</param>
		/// <example>this.SaveChangesWithTriggers(base.SaveChanges);</example>
		/// <returns>The number of objects written to the underlying database.</returns>
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
#if NET45 || NET451
		/// <summary>
		/// Asynchronously saves all changes made in this context to the underlying database, firing trigger events accordingly.
		/// </summary>
		/// <param name="dbContext"></param>
		/// <param name="baseSaveChangesAsync">A delegate to base.SaveChangesAsync(). Always pass `base.SaveChangesAsync`.</param>
		/// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> to observe while waiting for the task to complete.</param>
		/// <example>this.SaveChangesWithTriggersAsync(base.SaveChangesAsync);</example>
		/// <returns>A task that represents the asynchronous save operation. The task result contains the number of objects written to the underlying database.</returns>
		public static async Task<Int32> SaveChangesWithTriggersAsync(this DbContext dbContext, Func<CancellationToken, Task<Int32>> baseSaveChangesAsync, CancellationToken cancellationToken) {
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
#endif
	}
}