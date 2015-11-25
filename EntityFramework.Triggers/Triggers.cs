using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;

namespace EntityFramework.Triggers {
	internal static class Triggers {
		private static readonly ConcurrentDictionary<Type, Func<ITriggers>> triggersConstructorCache = new ConcurrentDictionary<Type, Func<ITriggers>>();

		public static ITriggers Create(ITriggerable triggerable) {
			var triggersConstructor = triggersConstructorCache.GetOrAdd(triggerable.GetType(), TriggersConstructorFactory);
			return triggersConstructor();
		}

		private static Func<ITriggers> TriggersConstructorFactory(Type triggerableType) {
			return Expression.Lambda<Func<ITriggers>>(Expression.New(typeof(Triggers<>).MakeGenericType(triggerableType))).Compile();
		}
	}

	public sealed class Triggers<TTriggerable> : ITriggers<TTriggerable>, ITriggers where TTriggerable : class, ITriggerable {
		internal Triggers() { }

		#region Entry implementations
		internal class Entry : IEntry<TTriggerable> {
			//[Obsolete("Please use the `Current` property. This property will be deprecated in the future.")]
			public TTriggerable Entity { get; internal set; }
			public DbContext Context { get; internal set; }
		}
		internal class AfterEntry : Entry, IAfterEntry<TTriggerable> { }

		internal class ChangeEntry : Entry, IChangeEntry<TTriggerable> {
			private readonly Lazy<TTriggerable> original;
			public TTriggerable Original => original.Value;
			public ChangeEntry() {
				original = new Lazy<TTriggerable>(() => DbPropertyValuesWrapper<TTriggerable>.Create(Context.Entry(Entity).OriginalValues));
			}
		}
		internal class AfterChangeEntry : ChangeEntry, IAfterChangeEntry<TTriggerable> { }

		internal class FailedEntry : Entry, IFailedEntry<TTriggerable> {
			public Exception Exception { get; internal set; }
		}
		internal class ChangeFailedEntry : ChangeEntry, IChangeFailedEntry<TTriggerable> {
			public Exception Exception { get; internal set; }
		}

		internal class InsertingEntry : Entry, IBeforeEntry<TTriggerable> {
			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
		}
		internal class UpdatingEntry : ChangeEntry, IBeforeChangeEntry<TTriggerable> {
			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
		}
		internal class DeletingEntry : ChangeEntry, IBeforeChangeEntry<TTriggerable> {
			public void Cancel() => Context.Entry(Entity).State = EntityState.Modified;
		}
		#endregion
		#region Event helpers
		private static void Add<TIEntry>(List<Action<TIEntry>> eventHandlers, Action<TIEntry> value) {
			lock (eventHandlers)
				eventHandlers.Add(value);
		}

		private static void Remove<TIEntry>(List<Action<TIEntry>> eventHandlers, Action<TIEntry> value) {
			lock (eventHandlers)
				eventHandlers.Remove(value);
		}

		private static void Raise<TIEntry>(List<Action<TIEntry>> eventHandlers, TIEntry entry) {
			List<Action<TIEntry>> eventHandlersCopy;
			lock (eventHandlers)
				eventHandlersCopy = new List<Action<TIEntry>>(eventHandlers);
			foreach (var eventHandler in eventHandlersCopy)
				eventHandler(entry);
		}
		#endregion
		#region Instance events
		private readonly List<Action<IBeforeEntry<TTriggerable>>> inserting = new List<Action<IBeforeEntry<TTriggerable>>>();
		private readonly List<Action<IBeforeChangeEntry<TTriggerable>>> updating = new List<Action<IBeforeChangeEntry<TTriggerable>>>();
		private readonly List<Action<IBeforeChangeEntry<TTriggerable>>> deleting = new List<Action<IBeforeChangeEntry<TTriggerable>>>();
		private readonly List<Action<IFailedEntry<TTriggerable>>> insertFailed = new List<Action<IFailedEntry<TTriggerable>>>();
		private readonly List<Action<IChangeFailedEntry<TTriggerable>>> updateFailed = new List<Action<IChangeFailedEntry<TTriggerable>>>();
		private readonly List<Action<IChangeFailedEntry<TTriggerable>>> deleteFailed = new List<Action<IChangeFailedEntry<TTriggerable>>>();
		private readonly List<Action<IAfterEntry<TTriggerable>>> inserted = new List<Action<IAfterEntry<TTriggerable>>>();
		private readonly List<Action<IAfterChangeEntry<TTriggerable>>> updated = new List<Action<IAfterChangeEntry<TTriggerable>>>();
		private readonly List<Action<IAfterChangeEntry<TTriggerable>>> deleted = new List<Action<IAfterChangeEntry<TTriggerable>>>();

		event Action<IBeforeEntry<TTriggerable>> ITriggers<TTriggerable>.Inserting { add { Add(inserting, value); } remove { Remove(inserting, value); } }
		event Action<IBeforeChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Updating { add { Add(updating, value); } remove { Remove(updating, value); } }
		event Action<IBeforeChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Deleting { add { Add(deleting, value); } remove { Remove(deleting, value); } }
		event Action<IFailedEntry<TTriggerable>> ITriggers<TTriggerable>.InsertFailed { add { Add(insertFailed, value); } remove { Remove(insertFailed, value); } }
		event Action<IChangeFailedEntry<TTriggerable>> ITriggers<TTriggerable>.UpdateFailed { add { Add(updateFailed, value); } remove { Remove(updateFailed, value); } }
		event Action<IChangeFailedEntry<TTriggerable>> ITriggers<TTriggerable>.DeleteFailed { add { Add(deleteFailed, value); } remove { Remove(deleteFailed, value); } }
		event Action<IAfterEntry<TTriggerable>> ITriggers<TTriggerable>.Inserted { add { Add(inserted, value); } remove { Remove(inserted, value); } }
		event Action<IAfterChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Updated { add { Add(updated, value); } remove { Remove(updated, value); } }
		event Action<IAfterChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Deleted { add { Add(deleted, value); } remove { Remove(deleted, value); } }

		void ITriggers.OnBeforeInsert(ITriggerable t, DbContext dbc) => Raise(inserting, new InsertingEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnBeforeUpdate(ITriggerable t, DbContext dbc) => Raise(updating, new UpdatingEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnBeforeDelete(ITriggerable t, DbContext dbc) => Raise(deleting, new DeletingEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnInsertFailed(ITriggerable t, DbContext dbc, Exception ex) => Raise(insertFailed, new FailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		void ITriggers.OnUpdateFailed(ITriggerable t, DbContext dbc, Exception ex) => Raise(updateFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		void ITriggers.OnDeleteFailed(ITriggerable t, DbContext dbc, Exception ex) => Raise(deleteFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		void ITriggers.OnAfterInsert(ITriggerable t, DbContext dbc) => Raise(inserted, new AfterEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnAfterUpdate(ITriggerable t, DbContext dbc) => Raise(updated, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnAfterDelete(ITriggerable t, DbContext dbc) => Raise(deleted, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		#endregion
		#region Static events
		private static readonly List<Action<IBeforeEntry<TTriggerable>>> staticInserting = new List<Action<IBeforeEntry<TTriggerable>>>();
		private static readonly List<Action<IBeforeChangeEntry<TTriggerable>>> staticUpdating = new List<Action<IBeforeChangeEntry<TTriggerable>>>();
		private static readonly List<Action<IBeforeChangeEntry<TTriggerable>>> staticDeleting = new List<Action<IBeforeChangeEntry<TTriggerable>>>();
		private static readonly List<Action<IFailedEntry<TTriggerable>>> staticInsertFailed = new List<Action<IFailedEntry<TTriggerable>>>();
		private static readonly List<Action<IChangeFailedEntry<TTriggerable>>> staticUpdateFailed = new List<Action<IChangeFailedEntry<TTriggerable>>>();
		private static readonly List<Action<IChangeFailedEntry<TTriggerable>>> staticDeleteFailed = new List<Action<IChangeFailedEntry<TTriggerable>>>();
		private static readonly List<Action<IAfterEntry<TTriggerable>>> staticInserted = new List<Action<IAfterEntry<TTriggerable>>>();
		private static readonly List<Action<IAfterChangeEntry<TTriggerable>>> staticUpdated = new List<Action<IAfterChangeEntry<TTriggerable>>>();
		private static readonly List<Action<IAfterChangeEntry<TTriggerable>>> staticDeleted = new List<Action<IAfterChangeEntry<TTriggerable>>>();

		public static event Action<IBeforeEntry<TTriggerable>> Inserting { add { Add(staticInserting, value); } remove { Remove(staticInserting, value); } }
		public static event Action<IBeforeEntry<TTriggerable>> Updating { add { Add(staticUpdating, value); } remove { Remove(staticUpdating, value); } }
		public static event Action<IBeforeEntry<TTriggerable>> Deleting { add { Add(staticDeleting, value); } remove { Remove(staticDeleting, value); } }
		public static event Action<IFailedEntry<TTriggerable>> InsertFailed { add { Add(staticInsertFailed, value); } remove { Remove(staticInsertFailed, value); } }
		public static event Action<IFailedEntry<TTriggerable>> UpdateFailed { add { Add(staticUpdateFailed, value); } remove { Remove(staticUpdateFailed, value); } }
		public static event Action<IFailedEntry<TTriggerable>> DeleteFailed { add { Add(staticDeleteFailed, value); } remove { Remove(staticDeleteFailed, value); } }
		public static event Action<IEntry<TTriggerable>> Inserted { add { Add(staticInserted, value); } remove { Remove(staticInserted, value); } }
		public static event Action<IChangeEntry<TTriggerable>> Updated { add { Add(staticUpdated, value); } remove { Remove(staticUpdated, value); } }
		public static event Action<IChangeEntry<TTriggerable>> Deleted { add { Add(staticDeleted, value); } remove { Remove(staticDeleted, value); } }

		internal static void OnBeforeInsertStatic(ITriggerable t, DbContext dbc) => Raise(staticInserting, new InsertingEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnBeforeUpdateStatic(ITriggerable t, DbContext dbc) => Raise(staticUpdating, new UpdatingEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnBeforeDeleteStatic(ITriggerable t, DbContext dbc) => Raise(staticDeleting, new DeletingEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnInsertFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Raise(staticInsertFailed, new FailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnUpdateFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Raise(staticUpdateFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnDeleteFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Raise(staticDeleteFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnAfterInsertStatic(ITriggerable t, DbContext dbc) => Raise(staticInserted, new AfterEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnAfterUpdateStatic(ITriggerable t, DbContext dbc) => Raise(staticUpdated, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnAfterDeleteStatic(ITriggerable t, DbContext dbc) => Raise(staticDeleted, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		#endregion
	}
}