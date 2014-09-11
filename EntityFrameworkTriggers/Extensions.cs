using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace EntityFrameworkTriggers {
	public static class Extensions {
		public static Triggers<TTriggerable> Triggers<TTriggerable>(this TTriggerable triggerable)
			where TTriggerable : class, ITriggerable<TTriggerable>, new()
		{
			return TriggersWeak<TTriggerable>.ConditionalWeakTable.GetValue(triggerable, key => new Triggers<TTriggerable> { Triggerable = triggerable });
		}

		private static readonly MethodInfo triggersMethodInfo = typeof(Extensions).GetMethod("Triggers");
		private static readonly Dictionary<Type, MethodInfo> triggerGenericMethodInfoCache = new Dictionary<Type, MethodInfo>();
		private static IEnumerable<Action<DbContext>> RaiseTheBeforeEvents(this DbContext dbContext) {
			var afterActions = new List<Action<DbContext>>();
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
						triggers.OnBeforeInsert(dbContext);
						afterActions.Add(triggers.OnAfterInsert);
						break;
					case EntityState.Deleted:
						triggers.OnBeforeDelete(dbContext);
						afterActions.Add(triggers.OnAfterDelete);
						break;
					case EntityState.Modified:
						triggers.OnBeforeUpdate(dbContext);
						afterActions.Add(triggers.OnAfterUpdate);
						break;
				}
			}
			return afterActions;
		}

		private static void RaiseTheAfterEvents(IEnumerable<Action<DbContext>> afterActions, DbContext dbContext) {
			foreach (var afterAction in afterActions)
				afterAction(dbContext);
		}

		public static Int32 SaveChangesWithTriggers(this DbContext dbContext) {
			var afterActions = dbContext.RaiseTheBeforeEvents();
			var result = dbContext.SaveChanges();
			RaiseTheAfterEvents(afterActions, dbContext);
			return result;
		}

		public static Task<Int32> SaveChangesWithTriggersAsync<TDbContext>(this TDbContext dbContext) where TDbContext : DbContext {
			return dbContext.SaveChangesWithTriggersAsync(CancellationToken.None);
		}

		public static async Task<Int32> SaveChangesWithTriggersAsync<TDbContext>(this TDbContext dbContext, CancellationToken cancellationToken) where TDbContext : DbContext {
			var afterActions = dbContext.RaiseTheBeforeEvents();
			var result = await dbContext.SaveChangesAsync(cancellationToken);
			RaiseTheAfterEvents(afterActions, dbContext);
			return result;
		}
	}

	public class Triggers<TTriggerable> : ITriggers where TTriggerable : class, ITriggerable<TTriggerable>, new() {
		/// <summary>Contains the context and the instance of the changed entity</summary>
		public struct Entry {
			/// <summary></summary>
			public DbContext Context { get; internal set; }
			public TDbContext GetContext<TDbContext>() where TDbContext : DbContext {
				return (TDbContext) Context;
			}
			/// <summary></summary>
			public TTriggerable Entity { get; internal set; }
		}

		internal TTriggerable Triggerable;
		
		/// <summary>Raised just before this entity is added to the store</summary>
		public event Action<Entry> Inserting;

		/// <summary>Raised just before this entity is updated in the store</summary>
		public event Action<Entry> Updating;

		/// <summary>Raised just before this entity is deleted from the store</summary>
		public event Action<Entry> Deleting;

		/// <summary>Raised just after this entity is added to the store</summary>
		public event Action<Entry> Inserted;

		/// <summary>Raised just after this entity is updated in the store</summary>
		public event Action<Entry> Updated;

		/// <summary>Raised just after this entity is deleted from the store</summary>
		public event Action<Entry> Deleted;

		private void RaiseDbEntityEntriesChangeEvent(Action<Entry> eventHandler, DbContext dbcontext) {
			if (eventHandler != null)
				eventHandler(new Entry { Context = dbcontext, Entity = Triggerable });
		}

		void ITriggers.OnBeforeInsert(DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Inserting, dbContext); }
		void ITriggers.OnBeforeUpdate(DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Updating, dbContext); }
		void ITriggers.OnBeforeDelete(DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Deleting, dbContext); }
		void ITriggers.OnAfterInsert(DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Inserted, dbContext); }
		void ITriggers.OnAfterUpdate(DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Updated, dbContext); }
		void ITriggers.OnAfterDelete(DbContext dbContext) { RaiseDbEntityEntriesChangeEvent(Deleted, dbContext); }
	}

	internal interface ITriggers {
		void OnBeforeInsert(DbContext dbContext);
		void OnBeforeUpdate(DbContext dbContext);
		void OnBeforeDelete(DbContext dbContext);
		void OnAfterInsert(DbContext dbContext);
		void OnAfterUpdate(DbContext dbContext);
		void OnAfterDelete(DbContext dbContext);
	}
}
