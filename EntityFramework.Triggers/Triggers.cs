using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Reflection;

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

		private static class Typed<TDbContext> {
			public static readonly ConcurrentDictionary<Type, Func<ITriggers, ITriggers>> triggersConstructorCache = new ConcurrentDictionary<Type, Func<ITriggers, ITriggers>>();
		}

		public static ITriggers Create<TDbContext>(ITriggerable triggerable) {
			var triggersConstructor = Typed<TDbContext>.triggersConstructorCache.GetOrAdd(triggerable.GetType(), TriggersConstructorFactory<TDbContext>);
			var triggers = triggerable.Triggers();
			return triggersConstructor((ITriggers) triggers);
		}

		private static Func<ITriggers, ITriggers> TriggersConstructorFactory<TDbContext>(Type triggerableType) {
			var parameter = Expression.Parameter(typeof (ITriggers));
			var genericTriggersType = typeof(Triggers<,>).MakeGenericType(triggerableType, typeof(TDbContext));
			var constructorInfo = genericTriggersType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(ITriggers) }, null);
			return Expression.Lambda<Func<ITriggers, ITriggers>>(Expression.New(constructorInfo, parameter), parameter).Compile();
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

		private static void Raise<TIEntry>(List<Action<TIEntry>> eventHandlers, TIEntry entry) {
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
		internal Triggers(ITriggers triggers) {
			//this.triggers = triggers;
			this.triggers = (Triggers<TTriggerable>) triggers;
		}

		private class Wrapper<T1, T2> {
			public Wrapper(Action<T2> outer, Action<T1> inner) {
				Outer = outer;
				Inner = inner;
			}

			public Action<T2> Outer { get; }
			public Action<T1> Inner { get; }
		}

		#region Event helpers
		private static void Add<T1, T2>(List<Wrapper<T1, T2>> wrappers, List<Action<T1>> eventHandlers, Action<T2> value, Func<T1, T2> factory) where T1 : class, IEntry<TTriggerable> where T2 : class {
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
				wrappers.Add(new Wrapper<T1, T2>(value, wrapper));
			}
		}

		private static void Remove<T1, T2>(List<Wrapper<T1, T2>> wrappers, List<Action<T1>> eventHandlers, Action<T2> value) where T1 : class where T2 : class {
			if (value == null)
				return;
			lock (eventHandlers) {
				var index = wrappers.FindLastIndex(x => x.Outer == value);
				if (index < 0)
					return;
				var wrapper = wrappers[index];
				wrappers.RemoveAt(index);
				index = eventHandlers.LastIndexOf(wrapper.Inner);
				eventHandlers.RemoveAt(index);
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
		private readonly List<Wrapper<IBeforeEntry      <TTriggerable>, IBeforeEntry      <TTriggerable, TDbContext>>> insertingWrappers    = new List<Wrapper<IBeforeEntry      <TTriggerable>, IBeforeEntry      <TTriggerable, TDbContext>>>();
		private readonly List<Wrapper<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>> updatingWrappers     = new List<Wrapper<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>>();
		private readonly List<Wrapper<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>> deletingWrappers     = new List<Wrapper<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>>();
		private readonly List<Wrapper<IFailedEntry      <TTriggerable>, IFailedEntry      <TTriggerable, TDbContext>>> insertFailedWrappers = new List<Wrapper<IFailedEntry      <TTriggerable>, IFailedEntry      <TTriggerable, TDbContext>>>();
		private readonly List<Wrapper<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>> updateFailedWrappers = new List<Wrapper<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>>();
		private readonly List<Wrapper<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>> deleteFailedWrappers = new List<Wrapper<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>>();
		private readonly List<Wrapper<IAfterEntry       <TTriggerable>, IAfterEntry       <TTriggerable, TDbContext>>> insertedWrappers     = new List<Wrapper<IAfterEntry       <TTriggerable>, IAfterEntry       <TTriggerable, TDbContext>>>();
		private readonly List<Wrapper<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>> updatedWrappers      = new List<Wrapper<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>>();
		private readonly List<Wrapper<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>> deletedWrappers      = new List<Wrapper<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>>();

		event Action<IBeforeEntry      <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Inserting    { add { Add(insertingWrappers   , triggers.inserting   , value, InsertingEntry   .Create); } remove { Remove(insertingWrappers   , triggers.inserting   , value); } }
		event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Updating     { add { Add(updatingWrappers    , triggers.updating    , value, UpdatingEntry    .Create); } remove { Remove(updatingWrappers    , triggers.updating    , value); } }
		event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Deleting     { add { Add(deletingWrappers    , triggers.deleting    , value, DeletingEntry    .Create); } remove { Remove(deletingWrappers    , triggers.deleting    , value); } }
		event Action<IFailedEntry      <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.InsertFailed { add { Add(insertFailedWrappers, triggers.insertFailed, value, FailedEntry      .Create); } remove { Remove(insertFailedWrappers, triggers.insertFailed, value); } }
		event Action<IChangeFailedEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.UpdateFailed { add { Add(updateFailedWrappers, triggers.updateFailed, value, ChangeFailedEntry.Create); } remove { Remove(updateFailedWrappers, triggers.updateFailed, value); } }
		event Action<IChangeFailedEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.DeleteFailed { add { Add(deleteFailedWrappers, triggers.deleteFailed, value, ChangeFailedEntry.Create); } remove { Remove(deleteFailedWrappers, triggers.deleteFailed, value); } }
		event Action<IAfterEntry       <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Inserted     { add { Add(insertedWrappers    , triggers.inserted    , value, AfterEntry       .Create); } remove { Remove(insertedWrappers    , triggers.inserted    , value); } }
		event Action<IAfterChangeEntry <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Updated      { add { Add(updatedWrappers     , triggers.updated     , value, AfterChangeEntry .Create); } remove { Remove(updatedWrappers     , triggers.updated     , value); } }
		event Action<IAfterChangeEntry <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Deleted      { add { Add(deletedWrappers     , triggers.deleted     , value, AfterChangeEntry .Create); } remove { Remove(deletedWrappers     , triggers.deleted     , value); } }

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
		private static readonly List<Wrapper<IBeforeEntry      <TTriggerable>, IBeforeEntry      <TTriggerable, TDbContext>>> staticInsertingWrappers    = new List<Wrapper<IBeforeEntry      <TTriggerable>, IBeforeEntry      <TTriggerable, TDbContext>>>();
		private static readonly List<Wrapper<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>> staticUpdatingWrappers     = new List<Wrapper<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>>();
		private static readonly List<Wrapper<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>> staticDeletingWrappers     = new List<Wrapper<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>>();
		private static readonly List<Wrapper<IFailedEntry      <TTriggerable>, IFailedEntry      <TTriggerable, TDbContext>>> staticInsertFailedWrappers = new List<Wrapper<IFailedEntry      <TTriggerable>, IFailedEntry      <TTriggerable, TDbContext>>>();
		private static readonly List<Wrapper<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>> staticUpdateFailedWrappers = new List<Wrapper<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>>();
		private static readonly List<Wrapper<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>> staticDeleteFailedWrappers = new List<Wrapper<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>>();
		private static readonly List<Wrapper<IAfterEntry       <TTriggerable>, IAfterEntry       <TTriggerable, TDbContext>>> staticInsertedWrappers     = new List<Wrapper<IAfterEntry       <TTriggerable>, IAfterEntry       <TTriggerable, TDbContext>>>();
		private static readonly List<Wrapper<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>> staticUpdatedWrappers      = new List<Wrapper<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>>();
		private static readonly List<Wrapper<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>> staticDeletedWrappers      = new List<Wrapper<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>>();

		public static event Action<IBeforeEntry      <TTriggerable, TDbContext>> Inserting    { add { Add(staticInsertingWrappers   , Triggers<TTriggerable>.staticInserting   , value, InsertingEntry   .Create); } remove { Remove(staticInsertingWrappers   , Triggers<TTriggerable>.staticInserting   , value); } }
		public static event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> Updating     { add { Add(staticUpdatingWrappers    , Triggers<TTriggerable>.staticUpdating    , value, UpdatingEntry    .Create); } remove { Remove(staticUpdatingWrappers    , Triggers<TTriggerable>.staticUpdating    , value); } }
		public static event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> Deleting     { add { Add(staticDeletingWrappers    , Triggers<TTriggerable>.staticDeleting    , value, DeletingEntry    .Create); } remove { Remove(staticDeletingWrappers    , Triggers<TTriggerable>.staticDeleting    , value); } }
		public static event Action<IFailedEntry      <TTriggerable, TDbContext>> InsertFailed { add { Add(staticInsertFailedWrappers, Triggers<TTriggerable>.staticInsertFailed, value, FailedEntry      .Create); } remove { Remove(staticInsertFailedWrappers, Triggers<TTriggerable>.staticInsertFailed, value); } }
		public static event Action<IChangeFailedEntry<TTriggerable, TDbContext>> UpdateFailed { add { Add(staticUpdateFailedWrappers, Triggers<TTriggerable>.staticUpdateFailed, value, ChangeFailedEntry.Create); } remove { Remove(staticUpdateFailedWrappers, Triggers<TTriggerable>.staticUpdateFailed, value); } }
		public static event Action<IChangeFailedEntry<TTriggerable, TDbContext>> DeleteFailed { add { Add(staticDeleteFailedWrappers, Triggers<TTriggerable>.staticDeleteFailed, value, ChangeFailedEntry.Create); } remove { Remove(staticDeleteFailedWrappers, Triggers<TTriggerable>.staticDeleteFailed, value); } }
		public static event Action<IAfterEntry       <TTriggerable, TDbContext>> Inserted     { add { Add(staticInsertedWrappers    , Triggers<TTriggerable>.staticInserted    , value, AfterEntry       .Create); } remove { Remove(staticInsertedWrappers    , Triggers<TTriggerable>.staticInserted    , value); } }
		public static event Action<IAfterChangeEntry <TTriggerable, TDbContext>> Updated      { add { Add(staticUpdatedWrappers     , Triggers<TTriggerable>.staticUpdated     , value, AfterChangeEntry .Create); } remove { Remove(staticUpdatedWrappers     , Triggers<TTriggerable>.staticUpdated     , value); } }
		public static event Action<IAfterChangeEntry <TTriggerable, TDbContext>> Deleted      { add { Add(staticDeletedWrappers     , Triggers<TTriggerable>.staticDeleted     , value, AfterChangeEntry .Create); } remove { Remove(staticDeletedWrappers     , Triggers<TTriggerable>.staticDeleted     , value); } }
		#endregion
	}
}