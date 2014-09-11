using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkTriggers {
	internal static class TriggersWeak<TTriggerable> where TTriggerable : class, ITriggerable<TTriggerable>, new() {
		public static readonly ConditionalWeakTable<TTriggerable, Triggers<TTriggerable>> ConditionalWeakTable = new ConditionalWeakTable<TTriggerable, Triggers<TTriggerable>>();
	}

	public static class Extensions {
		public static Triggers<TTriggerable> Triggers<TTriggerable>(this TTriggerable triggerable)
			where TTriggerable : class, ITriggerable<TTriggerable>, new()
		{
			return TriggersWeak<TTriggerable>.ConditionalWeakTable.GetValue(triggerable, key => new Triggers<TTriggerable> { Triggerable = triggerable });
		}

		private static readonly MethodInfo triggersMethodInfo = typeof(Extensions).GetMethod("Triggers");
		private static readonly Dictionary<Type, MethodInfo> triggerGenericMethodInfoCache = new Dictionary<Type, MethodInfo>();
		private static IEnumerable<Action> RaiseTheBeforeEvents(this DbContext dbContext) {
			var afterActions = new List<Action>();
			foreach (var entry in dbContext.ChangeTracker.Entries<ITriggerable>()) {
				var entityType = entry.Entity.GetType();
				MethodInfo triggerGenericMethodInfo;
				if (!triggerGenericMethodInfoCache.TryGetValue(entityType, out triggerGenericMethodInfo)) {
					triggerGenericMethodInfo = triggersMethodInfo.MakeGenericMethod(entityType);
					triggerGenericMethodInfoCache.Add(entityType, triggerGenericMethodInfo);
				}
				var triggers = (ITriggers)triggerGenericMethodInfo.Invoke(null, new[] { entry.Entity });
				switch (entry.State) {
					case EntityState.Added:
						triggers.OnBeforeInsert();
						afterActions.Add(triggers.OnAfterInsert);
						break;
					case EntityState.Deleted:
						triggers.OnBeforeDelete();
						afterActions.Add(triggers.OnAfterDelete);
						break;
					case EntityState.Modified:
						triggers.OnBeforeUpdate();
						afterActions.Add(triggers.OnAfterUpdate);
						break;
				}
			}
			return afterActions;
		}

		private static void RaiseTheAfterEvents(IEnumerable<Action> afterActions) {
			foreach (var afterAction in afterActions)
				afterAction();
		}

		public static Int32 SaveChangesWithTriggers(this DbContext dbContext) {
			var afterActions = dbContext.RaiseTheBeforeEvents();
			var result = dbContext.SaveChanges();
			RaiseTheAfterEvents(afterActions);
			return result;
		}

		public static Task<Int32> SaveChangesWithTriggersAsync<TDbContext>(this TDbContext dbContext) where TDbContext : DbContext {
			return dbContext.SaveChangesWithTriggersAsync(CancellationToken.None);
		}

		public static async Task<Int32> SaveChangesWithTriggersAsync<TDbContext>(this TDbContext dbContext, CancellationToken cancellationToken) where TDbContext : DbContext {
			var afterActions = dbContext.RaiseTheBeforeEvents();
			var result = await dbContext.SaveChangesAsync(cancellationToken);
			RaiseTheAfterEvents(afterActions);
			return result;
		}
	}

	public class Triggers<TTriggerable> : ITriggers where TTriggerable : class, ITriggerable<TTriggerable>, new()
	{
		internal TTriggerable Triggerable;
		
		/// <summary>Raised just before this entity is added to the store</summary>
		public event Action<TTriggerable> Inserting;

		/// <summary>Raised just before this entity is updated in the store</summary>
		public event Action<TTriggerable> Updating;

		/// <summary>Raised just before this entity is deleted from the store</summary>
		public event Action<TTriggerable> Deleting;

		/// <summary>Raised just after this entity is added to the store</summary>
		public event Action<TTriggerable> Inserted;

		/// <summary>Raised just after this entity is updated in the store</summary>
		public event Action<TTriggerable> Updated;

		/// <summary>Raised just after this entity is deleted from the store</summary>
		public event Action<TTriggerable> Deleted;

		private void RaiseDbEntityEntriesChangeEvent(Action<TTriggerable> eventHandler) {
			if (eventHandler != null)
				eventHandler(Triggerable);
		}

		void ITriggers.OnBeforeInsert() { RaiseDbEntityEntriesChangeEvent(Inserting); }
		void ITriggers.OnBeforeUpdate() { RaiseDbEntityEntriesChangeEvent(Updating); }
		void ITriggers.OnBeforeDelete() { RaiseDbEntityEntriesChangeEvent(Deleting); }
		void ITriggers.OnAfterInsert() { RaiseDbEntityEntriesChangeEvent(Inserted); }
		void ITriggers.OnAfterUpdate() { RaiseDbEntityEntriesChangeEvent(Updated); }
		void ITriggers.OnAfterDelete() { RaiseDbEntityEntriesChangeEvent(Deleted); }
	}

	internal interface ITriggers {
		void OnBeforeInsert();
		void OnBeforeUpdate();
		void OnBeforeDelete();
		void OnAfterInsert();
		void OnAfterUpdate();
		void OnAfterDelete();
	}
}
