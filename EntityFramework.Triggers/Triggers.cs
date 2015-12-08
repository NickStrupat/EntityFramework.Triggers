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

		#region Event helpers
		private static void Add<TIEntry>(List<Action<TIEntry>> eventHandlers, Action<TIEntry> value) {
			lock (eventHandlers)
				eventHandlers.Add(value);
		}

		private static void Remove<TIEntry>(List<Action<TIEntry>> eventHandlers, Action<TIEntry> value) {
			lock (eventHandlers)
				eventHandlers.Remove(value);
		}

		internal static void Raise<TIEntry>(List<Action<TIEntry>> eventHandlers, TIEntry entry) {
			lock (eventHandlers)
				foreach (var eventHandler in eventHandlers)
					eventHandler(entry);
		}
		#endregion
		#region Entry implementations
		private class Entry : IEntry<TTriggerable> {
			public TTriggerable Entity { get; internal set; }
			public DbContext Context { get; internal set; }
		}

		private class AfterEntry : Entry, IAfterEntry<TTriggerable> { }

		private abstract class ChangeEntry : Entry, IChangeEntry<TTriggerable> {
			private readonly Lazy<TTriggerable> original;
			public TTriggerable Original => original.Value;
			protected ChangeEntry() {
				original = new Lazy<TTriggerable>(() => DbPropertyValuesWrapper<TTriggerable>.Create(Context.Entry(Entity).OriginalValues));
			}
		}

		private class AfterChangeEntry : ChangeEntry, IAfterChangeEntry<TTriggerable> { }

		private class FailedEntry : Entry, IFailedEntry<TTriggerable> {
			public Exception Exception { get; internal set; }
		}

		private class ChangeFailedEntry : ChangeEntry, IChangeFailedEntry<TTriggerable> {
			public Exception Exception { get; internal set; }
		}

		private class InsertingEntry : Entry, IBeforeEntry<TTriggerable> {
			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
		}

		private class UpdatingEntry : ChangeEntry, IBeforeChangeEntry<TTriggerable> {
			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
		}

		private class DeletingEntry : ChangeEntry, IBeforeChangeEntry<TTriggerable> {
			public void Cancel() => Context.Entry(Entity).State = EntityState.Modified;
		}
		#endregion
		#region Instance events
		internal readonly List<Action<IBeforeEntry      <TTriggerable>>> inserting    = new List<Action<IBeforeEntry      <TTriggerable>>>();
		internal readonly List<Action<IBeforeChangeEntry<TTriggerable>>> updating     = new List<Action<IBeforeChangeEntry<TTriggerable>>>();
		internal readonly List<Action<IBeforeChangeEntry<TTriggerable>>> deleting     = new List<Action<IBeforeChangeEntry<TTriggerable>>>();
		internal readonly List<Action<IFailedEntry      <TTriggerable>>> insertFailed = new List<Action<IFailedEntry      <TTriggerable>>>();
		internal readonly List<Action<IChangeFailedEntry<TTriggerable>>> updateFailed = new List<Action<IChangeFailedEntry<TTriggerable>>>();
		internal readonly List<Action<IChangeFailedEntry<TTriggerable>>> deleteFailed = new List<Action<IChangeFailedEntry<TTriggerable>>>();
		internal readonly List<Action<IAfterEntry       <TTriggerable>>> inserted     = new List<Action<IAfterEntry       <TTriggerable>>>();
		internal readonly List<Action<IAfterChangeEntry <TTriggerable>>> updated      = new List<Action<IAfterChangeEntry <TTriggerable>>>();
		internal readonly List<Action<IAfterChangeEntry <TTriggerable>>> deleted      = new List<Action<IAfterChangeEntry <TTriggerable>>>();

		event Action<IBeforeEntry      <TTriggerable>> ITriggers<TTriggerable>.Inserting    { add { Add(inserting   , value); } remove { Remove(inserting   , value); } }
		event Action<IBeforeChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Updating     { add { Add(updating    , value); } remove { Remove(updating    , value); } }
		event Action<IBeforeChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Deleting     { add { Add(deleting    , value); } remove { Remove(deleting    , value); } }
		event Action<IFailedEntry      <TTriggerable>> ITriggers<TTriggerable>.InsertFailed { add { Add(insertFailed, value); } remove { Remove(insertFailed, value); } }
		event Action<IChangeFailedEntry<TTriggerable>> ITriggers<TTriggerable>.UpdateFailed { add { Add(updateFailed, value); } remove { Remove(updateFailed, value); } }
		event Action<IChangeFailedEntry<TTriggerable>> ITriggers<TTriggerable>.DeleteFailed { add { Add(deleteFailed, value); } remove { Remove(deleteFailed, value); } }
		event Action<IAfterEntry       <TTriggerable>> ITriggers<TTriggerable>.Inserted     { add { Add(inserted    , value); } remove { Remove(inserted    , value); } }
		event Action<IAfterChangeEntry <TTriggerable>> ITriggers<TTriggerable>.Updated      { add { Add(updated     , value); } remove { Remove(updated     , value); } }
		event Action<IAfterChangeEntry <TTriggerable>> ITriggers<TTriggerable>.Deleted      { add { Add(deleted     , value); } remove { Remove(deleted     , value); } }

		void ITriggers.OnBeforeInsert(ITriggerable t, DbContext dbc)               => Raise(inserting,    new InsertingEntry    { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnBeforeUpdate(ITriggerable t, DbContext dbc)               => Raise(updating,     new UpdatingEntry     { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnBeforeDelete(ITriggerable t, DbContext dbc)               => Raise(deleting,     new DeletingEntry     { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnInsertFailed(ITriggerable t, DbContext dbc, Exception ex) => Raise(insertFailed, new FailedEntry       { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		void ITriggers.OnUpdateFailed(ITriggerable t, DbContext dbc, Exception ex) => Raise(updateFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		void ITriggers.OnDeleteFailed(ITriggerable t, DbContext dbc, Exception ex) => Raise(deleteFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		void ITriggers.OnAfterInsert (ITriggerable t, DbContext dbc)               => Raise(inserted,     new AfterEntry        { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnAfterUpdate (ITriggerable t, DbContext dbc)               => Raise(updated,      new AfterChangeEntry  { Entity = (TTriggerable) t, Context = dbc });
		void ITriggers.OnAfterDelete (ITriggerable t, DbContext dbc)               => Raise(deleted,      new AfterChangeEntry  { Entity = (TTriggerable) t, Context = dbc });
		#endregion
		#region Static events
		internal static readonly List<Action<IBeforeEntry      <TTriggerable>>> staticInserting    = new List<Action<IBeforeEntry      <TTriggerable>>>();
		internal static readonly List<Action<IBeforeChangeEntry<TTriggerable>>> staticUpdating     = new List<Action<IBeforeChangeEntry<TTriggerable>>>();
		internal static readonly List<Action<IBeforeChangeEntry<TTriggerable>>> staticDeleting     = new List<Action<IBeforeChangeEntry<TTriggerable>>>();
		internal static readonly List<Action<IFailedEntry      <TTriggerable>>> staticInsertFailed = new List<Action<IFailedEntry      <TTriggerable>>>();
		internal static readonly List<Action<IChangeFailedEntry<TTriggerable>>> staticUpdateFailed = new List<Action<IChangeFailedEntry<TTriggerable>>>();
		internal static readonly List<Action<IChangeFailedEntry<TTriggerable>>> staticDeleteFailed = new List<Action<IChangeFailedEntry<TTriggerable>>>();
		internal static readonly List<Action<IAfterEntry       <TTriggerable>>> staticInserted     = new List<Action<IAfterEntry       <TTriggerable>>>();
		internal static readonly List<Action<IAfterChangeEntry <TTriggerable>>> staticUpdated      = new List<Action<IAfterChangeEntry <TTriggerable>>>();
		internal static readonly List<Action<IAfterChangeEntry <TTriggerable>>> staticDeleted      = new List<Action<IAfterChangeEntry <TTriggerable>>>();

		public static event Action<IBeforeEntry      <TTriggerable>> Inserting    { add { Add(staticInserting   , value); } remove { Remove(staticInserting   , value); } }
		public static event Action<IBeforeChangeEntry<TTriggerable>> Updating     { add { Add(staticUpdating    , value); } remove { Remove(staticUpdating    , value); } }
		public static event Action<IBeforeChangeEntry<TTriggerable>> Deleting     { add { Add(staticDeleting    , value); } remove { Remove(staticDeleting    , value); } }
		public static event Action<IFailedEntry      <TTriggerable>> InsertFailed { add { Add(staticInsertFailed, value); } remove { Remove(staticInsertFailed, value); } }
		public static event Action<IChangeFailedEntry<TTriggerable>> UpdateFailed { add { Add(staticUpdateFailed, value); } remove { Remove(staticUpdateFailed, value); } }
		public static event Action<IChangeFailedEntry<TTriggerable>> DeleteFailed { add { Add(staticDeleteFailed, value); } remove { Remove(staticDeleteFailed, value); } }
		public static event Action<IAfterEntry       <TTriggerable>> Inserted     { add { Add(staticInserted    , value); } remove { Remove(staticInserted    , value); } }
		public static event Action<IAfterChangeEntry <TTriggerable>> Updated      { add { Add(staticUpdated     , value); } remove { Remove(staticUpdated     , value); } }
		public static event Action<IAfterChangeEntry <TTriggerable>> Deleted      { add { Add(staticDeleted     , value); } remove { Remove(staticDeleted     , value); } }

		internal static void OnBeforeInsertStatic(ITriggerable t, DbContext dbc)               => Raise(staticInserting   , new InsertingEntry    { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnBeforeUpdateStatic(ITriggerable t, DbContext dbc)               => Raise(staticUpdating    , new UpdatingEntry     { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnBeforeDeleteStatic(ITriggerable t, DbContext dbc)               => Raise(staticDeleting    , new DeletingEntry     { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnInsertFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Raise(staticInsertFailed, new FailedEntry       { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnUpdateFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Raise(staticUpdateFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnDeleteFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Raise(staticDeleteFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
		internal static void OnAfterInsertStatic (ITriggerable t, DbContext dbc)               => Raise(staticInserted    , new AfterEntry        { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnAfterUpdateStatic (ITriggerable t, DbContext dbc)               => Raise(staticUpdated     , new AfterChangeEntry  { Entity = (TTriggerable) t, Context = dbc });
		internal static void OnAfterDeleteStatic (ITriggerable t, DbContext dbc)               => Raise(staticDeleted     , new AfterChangeEntry  { Entity = (TTriggerable) t, Context = dbc });
		#endregion
	}

	public sealed class Triggers<TTriggerable, TDbContext> : ITriggers<TTriggerable, TDbContext>, ITriggers where TDbContext : DbContext where TTriggerable : class, ITriggerable {
		private readonly Triggers<TTriggerable> triggers;
		internal Triggers(Triggers<TTriggerable> triggers) {
			this.triggers = triggers;
		}

		private class Wrapper<T1, T2> {
			public Wrapper(Action<T2> outer, Action<T1> inner) {
				Outer = outer;
				Inner = inner;
			}

			public Action<T2> Outer { get; }
			public Action<T1> Inner { get; }

			public static List<Wrapper<T1, T2>> Wrappers { get; } = new List<Wrapper<T1, T2>>();
		}

		#region Event helpers
		private static void Add<T1, T2>(List<Action<T1>> eventHandlers, Action<T2> value, Func<T1, T2> factory) where T1 : class, IEntry<TTriggerable> where T2 : class {
			if (value == null)
				return;
			lock (eventHandlers) {
				Action<T1> wrapper = x => {
					var context = x.Context as TDbContext;
					if (context == null)
						return;
					value(factory(x));
				};
				eventHandlers.Add(wrapper);
				Wrapper<T1, T2>.Wrappers.Add(new Wrapper<T1, T2>(value, wrapper));
			}
		}

		private static void Remove<T1, T2>(List<Action<T1>> eventHandlers, Action<T2> value) where T1 : class where T2 : class {
			if (value == null)
				return;
			lock (eventHandlers) {
				var index = Wrapper<T1, T2>.Wrappers.FindLastIndex(x => x.Outer == value);
				if (index < 0)
					return;
				var wrapper = Wrapper<T1, T2>.Wrappers[index];
				Wrapper<T1, T2>.Wrappers.RemoveAt(index);
				index = eventHandlers.LastIndexOf(wrapper.Inner);
				Wrapper<T1, T2>.Wrappers.RemoveAt(index);
			}
		}
		#endregion
		#region Entry implementations
		private class Entry : IEntry<TTriggerable, TDbContext> {
			protected Entry(IEntry<TTriggerable, DbContext> entry) {
				Entity = entry.Entity;
				Context = (TDbContext) entry.Context;
			}
			public TTriggerable Entity { get; }
			public TDbContext Context { get; }
		}

		private class AfterEntry : Entry, IAfterEntry<TTriggerable, TDbContext> {
			private AfterEntry(IEntry<TTriggerable, DbContext> entry) : base(entry) {}
			public static IAfterEntry<TTriggerable, TDbContext> Create(IAfterEntry<TTriggerable> entry) => new AfterEntry(entry);
		}

		private abstract class ChangeEntry : Entry, IChangeEntry<TTriggerable, TDbContext> {
			protected ChangeEntry(IEntry<TTriggerable, DbContext> entry) : base(entry) {
				original = new Lazy<TTriggerable>(() => DbPropertyValuesWrapper<TTriggerable>.Create(Context.Entry(Entity).OriginalValues));
			}

			private readonly Lazy<TTriggerable> original;
			public TTriggerable Original => original.Value;
		}

		private class AfterChangeEntry : ChangeEntry, IAfterChangeEntry<TTriggerable, TDbContext> {
			private AfterChangeEntry(IEntry<TTriggerable, DbContext> entry) : base(entry) {}
			public static IAfterChangeEntry<TTriggerable, TDbContext> Create(IAfterChangeEntry<TTriggerable> arg) => new AfterChangeEntry(arg);
		}

		private class FailedEntry : Entry, IFailedEntry<TTriggerable, TDbContext> {
			private FailedEntry(IEntry<TTriggerable, DbContext> entry) : base(entry) {}
			public Exception Exception { get; internal set; }
			public static IFailedEntry<TTriggerable, TDbContext> Create(IFailedEntry<TTriggerable> arg) => new FailedEntry(arg);
		}

		private class ChangeFailedEntry : ChangeEntry, IChangeFailedEntry<TTriggerable, TDbContext> {
			private ChangeFailedEntry(IEntry<TTriggerable, DbContext> entry) : base(entry) {}
			public Exception Exception { get; internal set; }
			public static IChangeFailedEntry<TTriggerable, TDbContext> Create(IChangeFailedEntry<TTriggerable> arg) => new ChangeFailedEntry(arg);
		}

		private class InsertingEntry : Entry, IBeforeEntry<TTriggerable, TDbContext> {
			private InsertingEntry(IBeforeEntry<TTriggerable> entry) : base(entry) {}
			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
			public static IBeforeEntry<TTriggerable, TDbContext> Create(IBeforeEntry<TTriggerable> entry) => new InsertingEntry(entry);
		}

		private class UpdatingEntry : ChangeEntry, IBeforeChangeEntry<TTriggerable, TDbContext> {
			private UpdatingEntry(IEntry<TTriggerable, DbContext> entry) : base(entry) {}
			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
			public static IBeforeChangeEntry<TTriggerable, TDbContext> Create(IBeforeChangeEntry<TTriggerable> entry) => new UpdatingEntry(entry);
		}

		private class DeletingEntry : ChangeEntry, IBeforeChangeEntry<TTriggerable, TDbContext> {
			private DeletingEntry(IEntry<TTriggerable, DbContext> entry) : base(entry) {}
			public void Cancel() => Context.Entry(Entity).State = EntityState.Modified;
			public static IBeforeChangeEntry<TTriggerable, TDbContext> Create(IBeforeChangeEntry<TTriggerable> entry) => new DeletingEntry(entry);
		}
		#endregion
		#region Instance events
		event Action<IBeforeEntry      <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Inserting    { add { Add(triggers.inserting   , value, InsertingEntry   .Create); } remove { Remove(triggers.inserting   , value); } }
		event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Updating     { add { Add(triggers.updating    , value, UpdatingEntry    .Create); } remove { Remove(triggers.updating    , value); } }
		event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Deleting     { add { Add(triggers.deleting    , value, DeletingEntry    .Create); } remove { Remove(triggers.deleting    , value); } }
		event Action<IFailedEntry      <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.InsertFailed { add { Add(triggers.insertFailed, value, FailedEntry      .Create); } remove { Remove(triggers.insertFailed, value); } }
		event Action<IChangeFailedEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.UpdateFailed { add { Add(triggers.updateFailed, value, ChangeFailedEntry.Create); } remove { Remove(triggers.updateFailed, value); } }
		event Action<IChangeFailedEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.DeleteFailed { add { Add(triggers.deleteFailed, value, ChangeFailedEntry.Create); } remove { Remove(triggers.deleteFailed, value); } }
		event Action<IAfterEntry       <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Inserted     { add { Add(triggers.inserted    , value, AfterEntry       .Create); } remove { Remove(triggers.inserted    , value); } }
		event Action<IAfterChangeEntry <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Updated      { add { Add(triggers.updated     , value, AfterChangeEntry .Create); } remove { Remove(triggers.updated     , value); } }
		event Action<IAfterChangeEntry <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Deleted      { add { Add(triggers.deleted     , value, AfterChangeEntry .Create); } remove { Remove(triggers.deleted     , value); } }

		void ITriggers.OnBeforeInsert(ITriggerable t, DbContext dbc)               => ((ITriggers) triggers).OnBeforeInsert(t, dbc);
		void ITriggers.OnBeforeUpdate(ITriggerable t, DbContext dbc)               => ((ITriggers) triggers).OnBeforeUpdate(t, dbc);
		void ITriggers.OnBeforeDelete(ITriggerable t, DbContext dbc)               => ((ITriggers) triggers).OnBeforeDelete(t, dbc);
		void ITriggers.OnInsertFailed(ITriggerable t, DbContext dbc, Exception ex) => ((ITriggers) triggers).OnInsertFailed(t, dbc, ex);
		void ITriggers.OnUpdateFailed(ITriggerable t, DbContext dbc, Exception ex) => ((ITriggers) triggers).OnUpdateFailed(t, dbc, ex);
		void ITriggers.OnDeleteFailed(ITriggerable t, DbContext dbc, Exception ex) => ((ITriggers) triggers).OnDeleteFailed(t, dbc, ex);
		void ITriggers.OnAfterInsert (ITriggerable t, DbContext dbc)               => ((ITriggers) triggers).OnAfterInsert (t, dbc);
		void ITriggers.OnAfterUpdate (ITriggerable t, DbContext dbc)               => ((ITriggers) triggers).OnAfterUpdate (t, dbc);
		void ITriggers.OnAfterDelete (ITriggerable t, DbContext dbc)               => ((ITriggers) triggers).OnAfterDelete (t, dbc);
		#endregion
		#region Static events
		public static event Action<IBeforeEntry      <TTriggerable, TDbContext>> Inserting    { add { Add(Triggers<TTriggerable>.staticInserting   , value, InsertingEntry   .Create); } remove { Remove(Triggers<TTriggerable>.staticInserting   , value); } }
		public static event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> Updating     { add { Add(Triggers<TTriggerable>.staticUpdating    , value, UpdatingEntry    .Create); } remove { Remove(Triggers<TTriggerable>.staticUpdating    , value); } }
		public static event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> Deleting     { add { Add(Triggers<TTriggerable>.staticDeleting    , value, DeletingEntry    .Create); } remove { Remove(Triggers<TTriggerable>.staticDeleting    , value); } }
		public static event Action<IFailedEntry      <TTriggerable, TDbContext>> InsertFailed { add { Add(Triggers<TTriggerable>.staticInsertFailed, value, FailedEntry      .Create); } remove { Remove(Triggers<TTriggerable>.staticInsertFailed, value); } }
		public static event Action<IChangeFailedEntry<TTriggerable, TDbContext>> UpdateFailed { add { Add(Triggers<TTriggerable>.staticUpdateFailed, value, ChangeFailedEntry.Create); } remove { Remove(Triggers<TTriggerable>.staticUpdateFailed, value); } }
		public static event Action<IChangeFailedEntry<TTriggerable, TDbContext>> DeleteFailed { add { Add(Triggers<TTriggerable>.staticDeleteFailed, value, ChangeFailedEntry.Create); } remove { Remove(Triggers<TTriggerable>.staticDeleteFailed, value); } }
		public static event Action<IAfterEntry       <TTriggerable, TDbContext>> Inserted     { add { Add(Triggers<TTriggerable>.staticInserted    , value, AfterEntry       .Create); } remove { Remove(Triggers<TTriggerable>.staticInserted    , value); } }
		public static event Action<IAfterChangeEntry <TTriggerable, TDbContext>> Updated      { add { Add(Triggers<TTriggerable>.staticUpdated     , value, AfterChangeEntry .Create); } remove { Remove(Triggers<TTriggerable>.staticUpdated     , value); } }
		public static event Action<IAfterChangeEntry <TTriggerable, TDbContext>> Deleted      { add { Add(Triggers<TTriggerable>.staticDeleted     , value, AfterChangeEntry .Create); } remove { Remove(Triggers<TTriggerable>.staticDeleted     , value); } }
		#endregion
	}
}