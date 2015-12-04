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
		#region Event helpers

		internal static void Add<TIEntry>(List<Action<TIEntry>> eventHandlers, Action<TIEntry> value) {
			lock (eventHandlers)
				eventHandlers.Add(value);
		}

		internal static void Remove<TIEntry>(List<Action<TIEntry>> eventHandlers, Action<TIEntry> value) {
			lock (eventHandlers)
				eventHandlers.Remove(value);
		}

		internal static void Raise<TIEntry>(List<Action<TIEntry>> eventHandlers, TIEntry entry) {
			List<Action<TIEntry>> eventHandlersCopy;
			lock (eventHandlers)
				eventHandlersCopy = new List<Action<TIEntry>>(eventHandlers);
			foreach (var eventHandler in eventHandlersCopy)
				eventHandler(entry);
		}
		#endregion
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

		event Action<IBeforeEntry<TTriggerable>> ITriggers<TTriggerable>.Inserting { add { Triggers.Add(inserting, value); } remove { Triggers.Remove(inserting, value); } }
		event Action<IBeforeChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Updating { add { Triggers.Add(updating, value); } remove { Triggers.Remove(updating, value); } }
		event Action<IBeforeChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Deleting { add { Triggers.Add(deleting, value); } remove { Triggers.Remove(deleting, value); } }
		event Action<IFailedEntry<TTriggerable>> ITriggers<TTriggerable>.InsertFailed { add { Triggers.Add(insertFailed, value); } remove { Triggers.Remove(insertFailed, value); } }
		event Action<IChangeFailedEntry<TTriggerable>> ITriggers<TTriggerable>.UpdateFailed { add { Triggers.Add(updateFailed, value); } remove { Triggers.Remove(updateFailed, value); } }
		event Action<IChangeFailedEntry<TTriggerable>> ITriggers<TTriggerable>.DeleteFailed { add { Triggers.Add(deleteFailed, value); } remove { Triggers.Remove(deleteFailed, value); } }
		event Action<IAfterEntry<TTriggerable>> ITriggers<TTriggerable>.Inserted { add { Triggers.Add(inserted, value); } remove { Triggers.Remove(inserted, value); } }
		event Action<IAfterChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Updated { add { Triggers.Add(updated, value); } remove { Triggers.Remove(updated, value); } }
		event Action<IAfterChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Deleted { add { Triggers.Add(deleted, value); } remove { Triggers.Remove(deleted, value); } }

		void ITriggers.OnBeforeInsert(ITriggerable t, DbContext dbc) => Triggers.Raise(inserting, new InsertingEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnBeforeUpdate(ITriggerable t, DbContext dbc) => Triggers.Raise(updating, new UpdatingEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnBeforeDelete(ITriggerable t, DbContext dbc) => Triggers.Raise(deleting, new DeletingEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnInsertFailed(ITriggerable t, DbContext dbc, Exception ex) => Triggers.Raise(insertFailed, new FailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		void ITriggers.OnUpdateFailed(ITriggerable t, DbContext dbc, Exception ex) => Triggers.Raise(updateFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		void ITriggers.OnDeleteFailed(ITriggerable t, DbContext dbc, Exception ex) => Triggers.Raise(deleteFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		void ITriggers.OnAfterInsert(ITriggerable t, DbContext dbc) => Triggers.Raise(inserted, new AfterEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnAfterUpdate(ITriggerable t, DbContext dbc) => Triggers.Raise(updated, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnAfterDelete(ITriggerable t, DbContext dbc) => Triggers.Raise(deleted, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
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

		public static event Action<IBeforeEntry<TTriggerable>> Inserting { add { Triggers.Add(staticInserting, value); } remove { Triggers.Remove(staticInserting, value); } }
		public static event Action<IBeforeEntry<TTriggerable>> Updating { add { Triggers.Add(staticUpdating, value); } remove { Triggers.Remove(staticUpdating, value); } }
		public static event Action<IBeforeEntry<TTriggerable>> Deleting { add { Triggers.Add(staticDeleting, value); } remove { Triggers.Remove(staticDeleting, value); } }
		public static event Action<IFailedEntry<TTriggerable>> InsertFailed { add { Triggers.Add(staticInsertFailed, value); } remove { Triggers.Remove(staticInsertFailed, value); } }
		public static event Action<IFailedEntry<TTriggerable>> UpdateFailed { add { Triggers.Add(staticUpdateFailed, value); } remove { Triggers.Remove(staticUpdateFailed, value); } }
		public static event Action<IFailedEntry<TTriggerable>> DeleteFailed { add { Triggers.Add(staticDeleteFailed, value); } remove { Triggers.Remove(staticDeleteFailed, value); } }
		public static event Action<IEntry<TTriggerable>> Inserted { add { Triggers.Add(staticInserted, value); } remove { Triggers.Remove(staticInserted, value); } }
		public static event Action<IChangeEntry<TTriggerable>> Updated { add { Triggers.Add(staticUpdated, value); } remove { Triggers.Remove(staticUpdated, value); } }
		public static event Action<IChangeEntry<TTriggerable>> Deleted { add { Triggers.Add(staticDeleted, value); } remove { Triggers.Remove(staticDeleted, value); } }

		internal static void OnBeforeInsertStatic(ITriggerable t, DbContext dbc) => Triggers.Raise(staticInserting, new InsertingEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnBeforeUpdateStatic(ITriggerable t, DbContext dbc) => Triggers.Raise(staticUpdating, new UpdatingEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnBeforeDeleteStatic(ITriggerable t, DbContext dbc) => Triggers.Raise(staticDeleting, new DeletingEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnInsertFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Triggers.Raise(staticInsertFailed, new FailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnUpdateFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Triggers.Raise(staticUpdateFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnDeleteFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Triggers.Raise(staticDeleteFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnAfterInsertStatic(ITriggerable t, DbContext dbc) => Triggers.Raise(staticInserted, new AfterEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnAfterUpdateStatic(ITriggerable t, DbContext dbc) => Triggers.Raise(staticUpdated, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnAfterDeleteStatic(ITriggerable t, DbContext dbc) => Triggers.Raise(staticDeleted, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		#endregion
	}

	public sealed class Triggers<TTriggerable, TDbContext> : ITriggers<TTriggerable, TDbContext>, ITriggers where TDbContext : DbContext where TTriggerable : class, ITriggerable
	{
		internal Triggers() {}

		#region Entry implementations
		internal class Entry : IEntry<TTriggerable, TDbContext> {
			public TTriggerable Entity { get; internal set; }
			public TDbContext Context { get; internal set; }
			DbContext IEntry<TTriggerable>.Context => Context;
		}
		internal class AfterEntry : Entry, IAfterEntry<TTriggerable, TDbContext> { }

		internal class ChangeEntry : Entry, IChangeEntry<TTriggerable, TDbContext> {
			private readonly Lazy<TTriggerable> original;
			public TTriggerable Original => original.Value;
			public ChangeEntry() {
				original = new Lazy<TTriggerable>(() => DbPropertyValuesWrapper<TTriggerable>.Create(Context.Entry(Entity).OriginalValues));
			}
		}
		internal class AfterChangeEntry : ChangeEntry, IAfterChangeEntry<TTriggerable, TDbContext> { }

		internal class FailedEntry : Entry, IFailedEntry<TTriggerable, TDbContext> {
			public Exception Exception { get; internal set; }
		}
		internal class ChangeFailedEntry : ChangeEntry, IChangeFailedEntry<TTriggerable, TDbContext> {
			public Exception Exception { get; internal set; }
		}

		internal class InsertingEntry : Entry, IBeforeEntry<TTriggerable, TDbContext> {
			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
		}
		internal class UpdatingEntry : ChangeEntry, IBeforeChangeEntry<TTriggerable, TDbContext> {
			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
		}
		internal class DeletingEntry : ChangeEntry, IBeforeChangeEntry<TTriggerable, TDbContext> {
			public void Cancel() => Context.Entry(Entity).State = EntityState.Modified;
		}
		#endregion
		#region Instance events
		private readonly List<Action<IBeforeEntry<TTriggerable, TDbContext>>> inserting = new List<Action<IBeforeEntry<TTriggerable, TDbContext>>>();
		private readonly List<Action<IBeforeChangeEntry<TTriggerable, TDbContext>>> updating = new List<Action<IBeforeChangeEntry<TTriggerable, TDbContext>>>();
		private readonly List<Action<IBeforeChangeEntry<TTriggerable, TDbContext>>> deleting = new List<Action<IBeforeChangeEntry<TTriggerable, TDbContext>>>();
		private readonly List<Action<IFailedEntry<TTriggerable, TDbContext>>> insertFailed = new List<Action<IFailedEntry<TTriggerable, TDbContext>>>();
		private readonly List<Action<IChangeFailedEntry<TTriggerable, TDbContext>>> updateFailed = new List<Action<IChangeFailedEntry<TTriggerable, TDbContext>>>();
		private readonly List<Action<IChangeFailedEntry<TTriggerable, TDbContext>>> deleteFailed = new List<Action<IChangeFailedEntry<TTriggerable, TDbContext>>>();
		private readonly List<Action<IAfterEntry<TTriggerable, TDbContext>>> inserted = new List<Action<IAfterEntry<TTriggerable, TDbContext>>>();
		private readonly List<Action<IAfterChangeEntry<TTriggerable, TDbContext>>> updated = new List<Action<IAfterChangeEntry<TTriggerable, TDbContext>>>();
		private readonly List<Action<IAfterChangeEntry<TTriggerable, TDbContext>>> deleted = new List<Action<IAfterChangeEntry<TTriggerable, TDbContext>>>();

		event Action<IBeforeEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Inserting { add { Triggers.Add(inserting, value); } remove { Triggers.Remove(inserting, value); } }
		event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Updating { add { Triggers.Add(updating, value); } remove { Triggers.Remove(updating, value); } }
		event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Deleting { add { Triggers.Add(deleting, value); } remove { Triggers.Remove(deleting, value); } }
		event Action<IFailedEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.InsertFailed { add { Triggers.Add(insertFailed, value); } remove { Triggers.Remove(insertFailed, value); } }
		event Action<IChangeFailedEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.UpdateFailed { add { Triggers.Add(updateFailed, value); } remove { Triggers.Remove(updateFailed, value); } }
		event Action<IChangeFailedEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.DeleteFailed { add { Triggers.Add(deleteFailed, value); } remove { Triggers.Remove(deleteFailed, value); } }
		event Action<IAfterEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Inserted { add { Triggers.Add(inserted, value); } remove { Triggers.Remove(inserted, value); } }
		event Action<IAfterChangeEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Updated { add { Triggers.Add(updated, value); } remove { Triggers.Remove(updated, value); } }
		event Action<IAfterChangeEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Deleted { add { Triggers.Add(deleted, value); } remove { Triggers.Remove(deleted, value); } }

		void ITriggers.OnBeforeInsert(ITriggerable t, DbContext dbc) => Triggers.Raise(inserting, new InsertingEntry { Entity = (TTriggerable) t, Context = (TDbContext) dbc });
		void ITriggers.OnBeforeUpdate(ITriggerable t, DbContext dbc) => Triggers.Raise(updating, new UpdatingEntry { Entity = (TTriggerable) t, Context = (TDbContext) dbc });
		void ITriggers.OnBeforeDelete(ITriggerable t, DbContext dbc) => Triggers.Raise(deleting, new DeletingEntry { Entity = (TTriggerable) t, Context = (TDbContext) dbc });
		void ITriggers.OnInsertFailed(ITriggerable t, DbContext dbc, Exception ex) => Triggers.Raise(insertFailed, new FailedEntry { Entity = (TTriggerable) t, Context = (TDbContext) dbc, Exception = ex });
		void ITriggers.OnUpdateFailed(ITriggerable t, DbContext dbc, Exception ex) => Triggers.Raise(updateFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = (TDbContext) dbc, Exception = ex });
		void ITriggers.OnDeleteFailed(ITriggerable t, DbContext dbc, Exception ex) => Triggers.Raise(deleteFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = (TDbContext) dbc, Exception = ex });
		void ITriggers.OnAfterInsert(ITriggerable t, DbContext dbc) => Triggers.Raise(inserted, new AfterEntry { Entity = (TTriggerable) t, Context = (TDbContext) dbc });
		void ITriggers.OnAfterUpdate(ITriggerable t, DbContext dbc) => Triggers.Raise(updated, new AfterChangeEntry { Entity = (TTriggerable) t, Context = (TDbContext) dbc });
		void ITriggers.OnAfterDelete(ITriggerable t, DbContext dbc) => Triggers.Raise(deleted, new AfterChangeEntry { Entity = (TTriggerable) t, Context = (TDbContext) dbc });
		#endregion
		#region Static events
		private static readonly List<Action<IBeforeEntry<TTriggerable, TDbContext>>> staticInserting = new List<Action<IBeforeEntry<TTriggerable, TDbContext>>>();
		private static readonly List<Action<IBeforeChangeEntry<TTriggerable, TDbContext>>> staticUpdating = new List<Action<IBeforeChangeEntry<TTriggerable, TDbContext>>>();
		private static readonly List<Action<IBeforeChangeEntry<TTriggerable, TDbContext>>> staticDeleting = new List<Action<IBeforeChangeEntry<TTriggerable, TDbContext>>>();
		private static readonly List<Action<IFailedEntry<TTriggerable, TDbContext>>> staticInsertFailed = new List<Action<IFailedEntry<TTriggerable, TDbContext>>>();
		private static readonly List<Action<IChangeFailedEntry<TTriggerable, TDbContext>>> staticUpdateFailed = new List<Action<IChangeFailedEntry<TTriggerable, TDbContext>>>();
		private static readonly List<Action<IChangeFailedEntry<TTriggerable, TDbContext>>> staticDeleteFailed = new List<Action<IChangeFailedEntry<TTriggerable, TDbContext>>>();
		private static readonly List<Action<IAfterEntry<TTriggerable, TDbContext>>> staticInserted = new List<Action<IAfterEntry<TTriggerable, TDbContext>>>();
		private static readonly List<Action<IAfterChangeEntry<TTriggerable, TDbContext>>> staticUpdated = new List<Action<IAfterChangeEntry<TTriggerable, TDbContext>>>();
		private static readonly List<Action<IAfterChangeEntry<TTriggerable, TDbContext>>> staticDeleted = new List<Action<IAfterChangeEntry<TTriggerable, TDbContext>>>();

		public static event Action<IBeforeEntry<TTriggerable, TDbContext>> Inserting { add { Triggers.Add(staticInserting, value); } remove { Triggers.Remove(staticInserting, value); } }
		public static event Action<IBeforeEntry<TTriggerable, TDbContext>> Updating { add { Triggers.Add(staticUpdating, value); } remove { Triggers.Remove(staticUpdating, value); } }
		public static event Action<IBeforeEntry<TTriggerable, TDbContext>> Deleting { add { Triggers.Add(staticDeleting, value); } remove { Triggers.Remove(staticDeleting, value); } }
		public static event Action<IFailedEntry<TTriggerable, TDbContext>> InsertFailed { add { Triggers.Add(staticInsertFailed, value); } remove { Triggers.Remove(staticInsertFailed, value); } }
		public static event Action<IFailedEntry<TTriggerable, TDbContext>> UpdateFailed { add { Triggers.Add(staticUpdateFailed, value); } remove { Triggers.Remove(staticUpdateFailed, value); } }
		public static event Action<IFailedEntry<TTriggerable, TDbContext>> DeleteFailed { add { Triggers.Add(staticDeleteFailed, value); } remove { Triggers.Remove(staticDeleteFailed, value); } }
		public static event Action<IEntry      <TTriggerable, TDbContext>> Inserted { add { Triggers.Add(staticInserted, value); } remove { Triggers.Remove(staticInserted, value); } }
		public static event Action<IChangeEntry<TTriggerable, TDbContext>> Updated { add { Triggers.Add(staticUpdated, value); } remove { Triggers.Remove(staticUpdated, value); } }
		public static event Action<IChangeEntry<TTriggerable, TDbContext>> Deleted { add { Triggers.Add(staticDeleted, value); } remove { Triggers.Remove(staticDeleted, value); } }

		internal static void OnBeforeInsertStatic(ITriggerable t, TDbContext dbc) => Triggers.Raise(staticInserting, new InsertingEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnBeforeUpdateStatic(ITriggerable t, TDbContext dbc) => Triggers.Raise(staticUpdating, new UpdatingEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnBeforeDeleteStatic(ITriggerable t, TDbContext dbc) => Triggers.Raise(staticDeleting, new DeletingEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnInsertFailedStatic(ITriggerable t, TDbContext dbc, Exception ex) => Triggers.Raise(staticInsertFailed, new FailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnUpdateFailedStatic(ITriggerable t, TDbContext dbc, Exception ex) => Triggers.Raise(staticUpdateFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnDeleteFailedStatic(ITriggerable t, TDbContext dbc, Exception ex) => Triggers.Raise(staticDeleteFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnAfterInsertStatic(ITriggerable t, TDbContext dbc) => Triggers.Raise(staticInserted, new AfterEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnAfterUpdateStatic(ITriggerable t, TDbContext dbc) => Triggers.Raise(staticUpdated, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnAfterDeleteStatic(ITriggerable t, TDbContext dbc) => Triggers.Raise(staticDeleted, new AfterChangeEntry { Entity = (TTriggerable) t, Context = dbc });
		#endregion
	}
}