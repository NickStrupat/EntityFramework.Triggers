using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
using EntityFrameworkCore.TypedOriginalValues;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
using EntityFramework.TypedOriginalValues;
namespace EntityFramework.Triggers {
#endif

	public static class Triggers<TEntity> where TEntity : class {
		public static event Action<IBeforeEntry      <TEntity, DbContext>> Inserting    { add { Triggers<TEntity, DbContext>.Inserting    += value; } remove { Triggers<TEntity, DbContext>.Inserting    -= value; } }
		public static event Action<IBeforeChangeEntry<TEntity, DbContext>> Updating     { add { Triggers<TEntity, DbContext>.Updating     += value; } remove { Triggers<TEntity, DbContext>.Updating     -= value; } }
		public static event Action<IBeforeChangeEntry<TEntity, DbContext>> Deleting     { add { Triggers<TEntity, DbContext>.Deleting     += value; } remove { Triggers<TEntity, DbContext>.Deleting     -= value; } }
		public static event Action<IFailedEntry      <TEntity, DbContext>> InsertFailed { add { Triggers<TEntity, DbContext>.InsertFailed += value; } remove { Triggers<TEntity, DbContext>.InsertFailed -= value; } }
		public static event Action<IChangeFailedEntry<TEntity, DbContext>> UpdateFailed { add { Triggers<TEntity, DbContext>.UpdateFailed += value; } remove { Triggers<TEntity, DbContext>.UpdateFailed -= value; } }
		public static event Action<IChangeFailedEntry<TEntity, DbContext>> DeleteFailed { add { Triggers<TEntity, DbContext>.DeleteFailed += value; } remove { Triggers<TEntity, DbContext>.DeleteFailed -= value; } }
		public static event Action<IAfterEntry       <TEntity, DbContext>> Inserted     { add { Triggers<TEntity, DbContext>.Inserted     += value; } remove { Triggers<TEntity, DbContext>.Inserted     -= value; } }
		public static event Action<IAfterChangeEntry <TEntity, DbContext>> Updated      { add { Triggers<TEntity, DbContext>.Updated      += value; } remove { Triggers<TEntity, DbContext>.Updated      -= value; } }
		public static event Action<IAfterChangeEntry <TEntity, DbContext>> Deleted      { add { Triggers<TEntity, DbContext>.Deleted      += value; } remove { Triggers<TEntity, DbContext>.Deleted      -= value; } }
	}

	public static class Triggers<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {
		#region Event helpers
		internal static void Add<TIEntry>(ref ImmutableArray<Action<TIEntry>> handlers, Action<TIEntry> handler) where TIEntry : IEntry<TEntity, TDbContext> {
			if (handler == null)
				return;

			ImmutableArray<Action<TIEntry>> initial, computed;
			do {
				initial = InterlockedGet(ref handlers);
				computed = initial.Add(handler);
			}
			while (initial != ImmutableInterlocked.InterlockedCompareExchange(ref handlers, computed, initial));
		}

		internal static void Remove<TIEntry>(ref ImmutableArray<Action<TIEntry>> handlers, Action<TIEntry> handler) where TIEntry : IEntry<TEntity, TDbContext> {
			if (handler == null)
				return;

			ImmutableArray<Action<TIEntry>> initial, computed;
			do {
				initial = InterlockedGet(ref handlers);
				computed = initial.Remove(handler);
			}
			while (initial != ImmutableInterlocked.InterlockedCompareExchange(ref handlers, computed, initial));
		}

		private static void Raise<TIEntry>(ref ImmutableArray<Action<TIEntry>> handlers, TIEntry entry) where TIEntry : IEntry<TEntity, TDbContext> {
			var latestHandlers = InterlockedGet(ref handlers);
			for (var i = 0; i < latestHandlers.Length; i++)
				latestHandlers[i].Invoke(entry);
		}

		private static ImmutableArray<Action<TIEntry>> InterlockedGet<TIEntry>(ref ImmutableArray<Action<TIEntry>> handlers) {
			return ImmutableInterlocked.InterlockedCompareExchange(ref handlers, ImmutableArray<Action<TIEntry>>.Empty, ImmutableArray<Action<TIEntry>>.Empty);
		}
		#endregion
		#region Events
		private static ImmutableArray<Action<IBeforeEntry      <TEntity, TDbContext>>> staticInserting    = ImmutableArray<Action<IBeforeEntry      <TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IBeforeChangeEntry<TEntity, TDbContext>>> staticUpdating     = ImmutableArray<Action<IBeforeChangeEntry<TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IBeforeChangeEntry<TEntity, TDbContext>>> staticDeleting     = ImmutableArray<Action<IBeforeChangeEntry<TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IFailedEntry      <TEntity, TDbContext>>> staticInsertFailed = ImmutableArray<Action<IFailedEntry      <TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IChangeFailedEntry<TEntity, TDbContext>>> staticUpdateFailed = ImmutableArray<Action<IChangeFailedEntry<TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IChangeFailedEntry<TEntity, TDbContext>>> staticDeleteFailed = ImmutableArray<Action<IChangeFailedEntry<TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IAfterEntry       <TEntity, TDbContext>>> staticInserted     = ImmutableArray<Action<IAfterEntry       <TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IAfterChangeEntry <TEntity, TDbContext>>> staticUpdated      = ImmutableArray<Action<IAfterChangeEntry <TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IAfterChangeEntry <TEntity, TDbContext>>> staticDeleted      = ImmutableArray<Action<IAfterChangeEntry <TEntity, TDbContext>>>.Empty;

		public static event Action<IBeforeEntry      <TEntity, TDbContext>> Inserting    { add { Add(ref staticInserting   , value); } remove { Remove(ref staticInserting   , value); } }
		public static event Action<IBeforeChangeEntry<TEntity, TDbContext>> Updating     { add { Add(ref staticUpdating    , value); } remove { Remove(ref staticUpdating    , value); } }
		public static event Action<IBeforeChangeEntry<TEntity, TDbContext>> Deleting     { add { Add(ref staticDeleting    , value); } remove { Remove(ref staticDeleting    , value); } }
		public static event Action<IFailedEntry      <TEntity, TDbContext>> InsertFailed { add { Add(ref staticInsertFailed, value); } remove { Remove(ref staticInsertFailed, value); } }
		public static event Action<IChangeFailedEntry<TEntity, TDbContext>> UpdateFailed { add { Add(ref staticUpdateFailed, value); } remove { Remove(ref staticUpdateFailed, value); } }
		public static event Action<IChangeFailedEntry<TEntity, TDbContext>> DeleteFailed { add { Add(ref staticDeleteFailed, value); } remove { Remove(ref staticDeleteFailed, value); } }
		public static event Action<IAfterEntry       <TEntity, TDbContext>> Inserted     { add { Add(ref staticInserted    , value); } remove { Remove(ref staticInserted    , value); } }
		public static event Action<IAfterChangeEntry <TEntity, TDbContext>> Updated      { add { Add(ref staticUpdated     , value); } remove { Remove(ref staticUpdated     , value); } }
		public static event Action<IAfterChangeEntry <TEntity, TDbContext>> Deleted      { add { Add(ref staticDeleted     , value); } remove { Remove(ref staticDeleted     , value); } }

		internal static void RaiseBeforeInsert(TEntity entity, TDbContext dbc)               => Raise(ref staticInserting   , new InsertingEntry    { Entity = entity, Context = dbc });
		internal static void RaiseBeforeUpdate(TEntity entity, TDbContext dbc)               => Raise(ref staticUpdating    , new UpdatingEntry     { Entity = entity, Context = dbc });
		internal static void RaiseBeforeDelete(TEntity entity, TDbContext dbc)               => Raise(ref staticDeleting    , new DeletingEntry     { Entity = entity, Context = dbc });
		internal static void RaiseInsertFailed(TEntity entity, TDbContext dbc, Exception ex) => Raise(ref staticInsertFailed, new FailedEntry       { Entity = entity, Context = dbc, Exception = ex });
		internal static void RaiseUpdateFailed(TEntity entity, TDbContext dbc, Exception ex) => Raise(ref staticUpdateFailed, new ChangeFailedEntry { Entity = entity, Context = dbc, Exception = ex });
		internal static void RaiseDeleteFailed(TEntity entity, TDbContext dbc, Exception ex) => Raise(ref staticDeleteFailed, new ChangeFailedEntry { Entity = entity, Context = dbc, Exception = ex });
		internal static void RaiseAfterInsert (TEntity entity, TDbContext dbc)               => Raise(ref staticInserted    , new AfterEntry        { Entity = entity, Context = dbc });
		internal static void RaiseAfterUpdate (TEntity entity, TDbContext dbc)               => Raise(ref staticUpdated     , new AfterChangeEntry  { Entity = entity, Context = dbc });
		internal static void RaiseAfterDelete (TEntity entity, TDbContext dbc)               => Raise(ref staticDeleted     , new AfterChangeEntry  { Entity = entity, Context = dbc });
		#endregion
		#region Entry implementations
		private class Entry : IEntry<TEntity, TDbContext> {
			public TEntity Entity { get; internal set; }
			public TDbContext Context { get; internal set; }
		}

		private class AfterEntry : Entry, IAfterEntry<TEntity, TDbContext> { }

		private abstract class ChangeEntry : Entry, IChangeEntry<TEntity, TDbContext> {
			private readonly Lazy<TEntity> original;
			public TEntity Original => original.Value;
			protected ChangeEntry() {
				original = new Lazy<TEntity>(() => Context.GetOriginal(Entity));
			}
		}

		private class AfterChangeEntry : ChangeEntry, IAfterChangeEntry<TEntity, TDbContext> { }

		private class FailedEntry : Entry, IFailedEntry<TEntity, TDbContext> {
			public Exception Exception { get; internal set; }
		}

		private class ChangeFailedEntry : ChangeEntry, IChangeFailedEntry<TEntity, TDbContext> {
			public Exception Exception { get; internal set; }
		}

		private class InsertingEntry : Entry, IBeforeEntry<TEntity, TDbContext> {
			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
		}

		private class UpdatingEntry : ChangeEntry, IBeforeChangeEntry<TEntity, TDbContext> {
			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
		}

		private class DeletingEntry : ChangeEntry, IBeforeChangeEntry<TEntity, TDbContext> {
			public void Cancel() => Context.Entry(Entity).State = EntityState.Modified;
		}
		#endregion
	}
}

//namespace EntityFramework.Triggers {
//	internal static class Triggers {
//		private static readonly ConcurrentDictionary<Type, Func<ITriggers>> triggersConstructorCache = new ConcurrentDictionary<Type, Func<ITriggers>>();

//		public static ITriggers Create(ITriggerable triggerable) {
//			var triggersConstructor = triggersConstructorCache.GetOrAdd(triggerable.GetType(), TriggersConstructorFactory);
//			return triggersConstructor();
//		}

//		private static Func<ITriggers> TriggersConstructorFactory(Type triggerableType) {
//			return Expression.Lambda<Func<ITriggers>>(Expression.New(typeof(Triggers<>).MakeGenericType(triggerableType))).Compile();
//		}

//		private static class Typed<TDbContext> {
//			public static readonly ConcurrentDictionary<Type, Func<ITriggers, ITriggers>> triggersConstructorCache = new ConcurrentDictionary<Type, Func<ITriggers, ITriggers>>();
//		}

//		public static ITriggers Create<TDbContext>(ITriggerable triggerable) {
//			var triggersConstructor = Typed<TDbContext>.triggersConstructorCache.GetOrAdd(triggerable.GetType(), TriggersConstructorFactory<TDbContext>);
//			var triggers = triggerable.Triggers();
//			return triggersConstructor((ITriggers) triggers);
//		}

//		private static Func<ITriggers, ITriggers> TriggersConstructorFactory<TDbContext>(Type triggerableType) {
//			var parameter = Expression.Parameter(typeof (ITriggers));
//			var genericTriggersType = typeof(Triggers<,>).MakeGenericType(triggerableType, typeof(TDbContext));
//			var constructorInfo = genericTriggersType.GetConstructor(new[] {typeof(ITriggers)});
//			//var constructorInfo = genericTriggersType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(ITriggers) }, null);
//			return Expression.Lambda<Func<ITriggers, ITriggers>>(Expression.New(constructorInfo, parameter), parameter).Compile();
//		}
//	}

//	internal struct Handler<TIEntry, TIEntryHelper> {
//		public Handler(Action<TIEntry> main, Action<TIEntryHelper> helper = null) {
//			Main = main;
//			Helper = helper;
//		}

//		public Action<TIEntry>       Main   { get; }
//		public Action<TIEntryHelper> Helper { get; }

//		public static readonly MainEqualityComparer MainComparer = new MainEqualityComparer();
//		public static readonly HelperEqualityComparer HelperComparer = new HelperEqualityComparer();

//		internal class MainEqualityComparer : IEqualityComparer<Handler<TIEntry, TIEntryHelper>> {
//			public Boolean Equals(Handler<TIEntry, TIEntryHelper> x, Handler<TIEntry, TIEntryHelper> y) => x.Main.Equals(y.Main);
//			public Int32 GetHashCode(Handler<TIEntry, TIEntryHelper> obj) => obj.Main.GetHashCode();
//		}

//		internal class HelperEqualityComparer : IEqualityComparer<Handler<TIEntry, TIEntryHelper>> {
//			public Boolean Equals(Handler<TIEntry, TIEntryHelper> x, Handler<TIEntry, TIEntryHelper> y) => x.Helper == y.Helper;
//			public Int32 GetHashCode(Handler<TIEntry, TIEntryHelper> obj) => obj.Helper.GetHashCode();
//		}
//	}

//	public sealed class Triggers<TTriggerable> : ITriggers<TTriggerable>, ITriggers where TTriggerable : class, ITriggerable {
//		internal Triggers() { }

//		#region Event helpers
//		//private static void Add<TIEntry>(List<Action<TIEntry>> eventHandlers, Action<TIEntry> value) {
//		//	lock (eventHandlers)
//		//		eventHandlers.Add(value);
//		//}

//		//private static void Remove<TIEntry>(List<Action<TIEntry>> eventHandlers, Action<TIEntry> value) {
//		//	lock (eventHandlers)
//		//		eventHandlers.Remove(value);
//		//}

//		//private static void Raise<TIEntry>(List<Action<TIEntry>> eventHandlers, TIEntry entry) {
//		//	lock (eventHandlers)
//		//		foreach (var eventHandler in eventHandlers)
//		//			eventHandler(entry);
//		//}

//		internal static ImmutableArray<Handler<T1, T2>> InterlockedGet<T1, T2>(ref ImmutableArray<Handler<T1, T2>> handlers) {
//			return ImmutableInterlocked.InterlockedCompareExchange(ref handlers, ImmutableArray<Handler<T1, T2>>.Empty, ImmutableArray<Handler<T1, T2>>.Empty);
//		}

//		private static void Add<TIEntry, TIEntryHelper>(ref ImmutableArray<Handler<TIEntry, TIEntryHelper>> handlers, Action<TIEntry> handler, IEqualityComparer<Handler<TIEntry, TIEntryHelper>> comparer, Action<TIEntryHelper> helper = null) {
//			if (handler == null)
//				return;

//			ImmutableArray<Handler<TIEntry, TIEntryHelper>> initial, computed;
//			do {
//				initial = InterlockedGet(ref handlers);
//				computed = initial.Add(new Handler<TIEntry, TIEntryHelper>(handler));
//			}
//			while (initial != ImmutableInterlocked.InterlockedCompareExchange(ref handlers, computed, initial));
//		}

//		private static void Remove<TIEntry, TIEntryHelper>(ref ImmutableArray<Handler<TIEntry, TIEntryHelper>> handlers, Action<TIEntry> handler, IEqualityComparer<Handler<TIEntry, TIEntryHelper>> comparer, Action<TIEntryHelper> helper = null) {
//			if (handler == null)
//				return;

//			ImmutableArray<Handler<TIEntry, TIEntryHelper>> initial, computed;
//			do {
//				initial = InterlockedGet(ref handlers);
//				computed = initial.Remove(new Handler<TIEntry, TIEntryHelper>(handler));
//			}
//			while (initial != ImmutableInterlocked.InterlockedCompareExchange(ref handlers, computed, initial));
//		}

//		private static void Raise<TIEntry, TIEntryHelper>(ref ImmutableArray<Handler<TIEntry, TIEntryHelper>> handlers, TIEntry entry) {
//			var latestHandlers = InterlockedGet(ref handlers);
//			for (var i = 0; i < latestHandlers.Length; i++)
//				latestHandlers[i].Main(entry);
//		}

//		//private static void Add<TIEntry>(ref ImmutableList<Action<TIEntry>> handlers, Action<TIEntry> handler) {
//		//	Interlocked.Exchange(ref handlers, handlers.Add(handler));
//		//}

//		//private static void Remove<TIEntry>(ref ImmutableList<Action<TIEntry>> handlers, Action<TIEntry> handler) {
//		//	Interlocked.Exchange(ref handlers, handlers.Remove(handler));
//		//}

//		//private static void Raise<TIEntry>(ref ImmutableList<Action<TIEntry>> handlers, TIEntry entry) {
//		//	var latestHandlers = Interlocked.CompareExchange(ref handlers, null, null);
//		//	foreach (var handler in latestHandlers)
//		//		handler(entry);
//		//}
//		#endregion
//		#region Entry implementations
//		private class Entry : IEntry<TTriggerable> {
//			public TTriggerable Entity { get; internal set; }
//			public DbContext Context { get; internal set; }
//		}

//		private class AfterEntry : Entry, IAfterEntry<TTriggerable> { }

//		private abstract class ChangeEntry : Entry, IChangeEntry<TTriggerable> {
//			private readonly Lazy<TTriggerable> original;
//			public TTriggerable Original => original.Value;
//			protected ChangeEntry() {
//				original = new Lazy<TTriggerable>(() => Context.GetOriginal(Entity));
//			}
//		}

//		private class AfterChangeEntry : ChangeEntry, IAfterChangeEntry<TTriggerable> { }

//		private class FailedEntry : Entry, IFailedEntry<TTriggerable> {
//			public Exception Exception { get; internal set; }
//		}

//		private class ChangeFailedEntry : ChangeEntry, IChangeFailedEntry<TTriggerable> {
//			public Exception Exception { get; internal set; }
//		}

//		private class InsertingEntry : Entry, IBeforeEntry<TTriggerable> {
//			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
//		}

//		private class UpdatingEntry : ChangeEntry, IBeforeChangeEntry<TTriggerable> {
//			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
//		}

//		private class DeletingEntry : ChangeEntry, IBeforeChangeEntry<TTriggerable> {
//			public void Cancel() => Context.Entry(Entity).State = EntityState.Modified;
//		}
//		#endregion
//		#region Instance events
//		internal ImmutableArray<Handler<IBeforeEntry      <TTriggerable>, Object>> inserting    = ImmutableArray<Handler<IBeforeEntry      <TTriggerable>, Object>>.Empty;
//		internal ImmutableArray<Handler<IBeforeChangeEntry<TTriggerable>, Object>> updating     = ImmutableArray<Handler<IBeforeChangeEntry<TTriggerable>, Object>>.Empty;
//		internal ImmutableArray<Handler<IBeforeChangeEntry<TTriggerable>, Object>> deleting     = ImmutableArray<Handler<IBeforeChangeEntry<TTriggerable>, Object>>.Empty;
//		internal ImmutableArray<Handler<IFailedEntry      <TTriggerable>, Object>> insertFailed = ImmutableArray<Handler<IFailedEntry      <TTriggerable>, Object>>.Empty;
//		internal ImmutableArray<Handler<IChangeFailedEntry<TTriggerable>, Object>> updateFailed = ImmutableArray<Handler<IChangeFailedEntry<TTriggerable>, Object>>.Empty;
//		internal ImmutableArray<Handler<IChangeFailedEntry<TTriggerable>, Object>> deleteFailed = ImmutableArray<Handler<IChangeFailedEntry<TTriggerable>, Object>>.Empty;
//		internal ImmutableArray<Handler<IAfterEntry       <TTriggerable>, Object>> inserted     = ImmutableArray<Handler<IAfterEntry       <TTriggerable>, Object>>.Empty;
//		internal ImmutableArray<Handler<IAfterChangeEntry <TTriggerable>, Object>> updated      = ImmutableArray<Handler<IAfterChangeEntry <TTriggerable>, Object>>.Empty;
//		internal ImmutableArray<Handler<IAfterChangeEntry <TTriggerable>, Object>> deleted      = ImmutableArray<Handler<IAfterChangeEntry <TTriggerable>, Object>>.Empty;

//		event Action<IBeforeEntry      <TTriggerable>> ITriggers<TTriggerable>.Inserting    { add { Add(ref inserting   , value); } remove { Remove(ref inserting   , value); } }
//		event Action<IBeforeChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Updating     { add { Add(ref updating    , value); } remove { Remove(ref updating    , value); } }
//		event Action<IBeforeChangeEntry<TTriggerable>> ITriggers<TTriggerable>.Deleting     { add { Add(ref deleting    , value); } remove { Remove(ref deleting    , value); } }
//		event Action<IFailedEntry      <TTriggerable>> ITriggers<TTriggerable>.InsertFailed { add { Add(ref insertFailed, value); } remove { Remove(ref insertFailed, value); } }
//		event Action<IChangeFailedEntry<TTriggerable>> ITriggers<TTriggerable>.UpdateFailed { add { Add(ref updateFailed, value); } remove { Remove(ref updateFailed, value); } }
//		event Action<IChangeFailedEntry<TTriggerable>> ITriggers<TTriggerable>.DeleteFailed { add { Add(ref deleteFailed, value); } remove { Remove(ref deleteFailed, value); } }
//		event Action<IAfterEntry       <TTriggerable>> ITriggers<TTriggerable>.Inserted     { add { Add(ref inserted    , value); } remove { Remove(ref inserted    , value); } }
//		event Action<IAfterChangeEntry <TTriggerable>> ITriggers<TTriggerable>.Updated      { add { Add(ref updated     , value); } remove { Remove(ref updated     , value); } }
//		event Action<IAfterChangeEntry <TTriggerable>> ITriggers<TTriggerable>.Deleted      { add { Add(ref deleted     , value); } remove { Remove(ref deleted     , value); } }

//		void ITriggers.OnBeforeInsert(ITriggerable t, DbContext dbc)               => Raise(ref inserting,    new InsertingEntry    { Entity = (TTriggerable) t, Context = dbc });
//		void ITriggers.OnBeforeUpdate(ITriggerable t, DbContext dbc)               => Raise(ref updating,     new UpdatingEntry     { Entity = (TTriggerable) t, Context = dbc });
//		void ITriggers.OnBeforeDelete(ITriggerable t, DbContext dbc)               => Raise(ref deleting,     new DeletingEntry     { Entity = (TTriggerable) t, Context = dbc });
//		void ITriggers.OnInsertFailed(ITriggerable t, DbContext dbc, Exception ex) => Raise(ref insertFailed, new FailedEntry       { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
//		void ITriggers.OnUpdateFailed(ITriggerable t, DbContext dbc, Exception ex) => Raise(ref updateFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
//		void ITriggers.OnDeleteFailed(ITriggerable t, DbContext dbc, Exception ex) => Raise(ref deleteFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
//		void ITriggers.OnAfterInsert (ITriggerable t, DbContext dbc)               => Raise(ref inserted,     new AfterEntry        { Entity = (TTriggerable) t, Context = dbc });
//		void ITriggers.OnAfterUpdate (ITriggerable t, DbContext dbc)               => Raise(ref updated,      new AfterChangeEntry  { Entity = (TTriggerable) t, Context = dbc });
//		void ITriggers.OnAfterDelete (ITriggerable t, DbContext dbc)               => Raise(ref deleted,      new AfterChangeEntry  { Entity = (TTriggerable) t, Context = dbc });
//		#endregion
//		#region Static events
//		internal static ImmutableArray<Handler<IBeforeEntry      <TTriggerable>, Object>> staticInserting    = ImmutableArray<Handler<IBeforeEntry      <TTriggerable>, Object>>.Empty;
//		internal static ImmutableArray<Handler<IBeforeChangeEntry<TTriggerable>, Object>> staticUpdating     = ImmutableArray<Handler<IBeforeChangeEntry<TTriggerable>, Object>>.Empty;
//		internal static ImmutableArray<Handler<IBeforeChangeEntry<TTriggerable>, Object>> staticDeleting     = ImmutableArray<Handler<IBeforeChangeEntry<TTriggerable>, Object>>.Empty;
//		internal static ImmutableArray<Handler<IFailedEntry      <TTriggerable>, Object>> staticInsertFailed = ImmutableArray<Handler<IFailedEntry      <TTriggerable>, Object>>.Empty;
//		internal static ImmutableArray<Handler<IChangeFailedEntry<TTriggerable>, Object>> staticUpdateFailed = ImmutableArray<Handler<IChangeFailedEntry<TTriggerable>, Object>>.Empty;
//		internal static ImmutableArray<Handler<IChangeFailedEntry<TTriggerable>, Object>> staticDeleteFailed = ImmutableArray<Handler<IChangeFailedEntry<TTriggerable>, Object>>.Empty;
//		internal static ImmutableArray<Handler<IAfterEntry       <TTriggerable>, Object>> staticInserted     = ImmutableArray<Handler<IAfterEntry       <TTriggerable>, Object>>.Empty;
//		internal static ImmutableArray<Handler<IAfterChangeEntry <TTriggerable>, Object>> staticUpdated      = ImmutableArray<Handler<IAfterChangeEntry <TTriggerable>, Object>>.Empty;
//		internal static ImmutableArray<Handler<IAfterChangeEntry <TTriggerable>, Object>> staticDeleted      = ImmutableArray<Handler<IAfterChangeEntry <TTriggerable>, Object>>.Empty;

//		public static event Action<IBeforeEntry      <TTriggerable>> Inserting    { add { Add(ref staticInserting   , value); } remove { Remove(ref staticInserting   , value); } }
//		public static event Action<IBeforeChangeEntry<TTriggerable>> Updating     { add { Add(ref staticUpdating    , value); } remove { Remove(ref staticUpdating    , value); } }
//		public static event Action<IBeforeChangeEntry<TTriggerable>> Deleting     { add { Add(ref staticDeleting    , value); } remove { Remove(ref staticDeleting    , value); } }
//		public static event Action<IFailedEntry      <TTriggerable>> InsertFailed { add { Add(ref staticInsertFailed, value); } remove { Remove(ref staticInsertFailed, value); } }
//		public static event Action<IChangeFailedEntry<TTriggerable>> UpdateFailed { add { Add(ref staticUpdateFailed, value); } remove { Remove(ref staticUpdateFailed, value); } }
//		public static event Action<IChangeFailedEntry<TTriggerable>> DeleteFailed { add { Add(ref staticDeleteFailed, value); } remove { Remove(ref staticDeleteFailed, value); } }
//		public static event Action<IAfterEntry       <TTriggerable>> Inserted     { add { Add(ref staticInserted    , value); } remove { Remove(ref staticInserted    , value); } }
//		public static event Action<IAfterChangeEntry <TTriggerable>> Updated      { add { Add(ref staticUpdated     , value); } remove { Remove(ref staticUpdated     , value); } }
//		public static event Action<IAfterChangeEntry <TTriggerable>> Deleted      { add { Add(ref staticDeleted     , value); } remove { Remove(ref staticDeleted     , value); } }

//		internal static void OnBeforeInsertStatic(ITriggerable t, DbContext dbc)               => Raise(ref staticInserting   , new InsertingEntry    { Entity = (TTriggerable) t, Context = dbc });
//		internal static void OnBeforeUpdateStatic(ITriggerable t, DbContext dbc)               => Raise(ref staticUpdating    , new UpdatingEntry     { Entity = (TTriggerable) t, Context = dbc });
//		internal static void OnBeforeDeleteStatic(ITriggerable t, DbContext dbc)               => Raise(ref staticDeleting    , new DeletingEntry     { Entity = (TTriggerable) t, Context = dbc });
//		internal static void OnInsertFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Raise(ref staticInsertFailed, new FailedEntry       { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
//		internal static void OnUpdateFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Raise(ref staticUpdateFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
//		internal static void OnDeleteFailedStatic(ITriggerable t, DbContext dbc, Exception ex) => Raise(ref staticDeleteFailed, new ChangeFailedEntry { Entity = (TTriggerable) t, Context = dbc, Exception = ex });
//		internal static void OnAfterInsertStatic (ITriggerable t, DbContext dbc)               => Raise(ref staticInserted    , new AfterEntry        { Entity = (TTriggerable) t, Context = dbc });
//		internal static void OnAfterUpdateStatic (ITriggerable t, DbContext dbc)               => Raise(ref staticUpdated     , new AfterChangeEntry  { Entity = (TTriggerable) t, Context = dbc });
//		internal static void OnAfterDeleteStatic (ITriggerable t, DbContext dbc)               => Raise(ref staticDeleted     , new AfterChangeEntry  { Entity = (TTriggerable) t, Context = dbc });
//		#endregion
//	}

//	public sealed class Triggers<TTriggerable, TDbContext> : ITriggers<TTriggerable, TDbContext>, ITriggers where TDbContext : DbContext where TTriggerable : class, ITriggerable {
//		private readonly Triggers<TTriggerable> triggers;
//		internal Triggers(ITriggers triggers) {
//			//this.triggers = triggers;
//			this.triggers = (Triggers<TTriggerable>) triggers;
//		}

//		//private struct Wrapper<T1, T2> {
//		//	public Wrapper(Action<T2> outer, Action<T1> inner) {
//		//		Outer = outer;
//		//		Inner = inner;
//		//	}

//		//	public Action<T2> Outer { get; }
//		//	public Action<T1> Inner { get; }
//		//}

//		#region Event helpers
//		//private static void Add<T1, T2>(List<Wrapper<T1, T2>> wrappers, List<Action<T1>> handlers, Action<T2> handler, Func<T1, T2> factory) where T1 : class, IEntry<TTriggerable> where T2 : class {
//		//	if (handler == null)
//		//		return;
//		//	lock (handlers) {
//		//		Action<T1> wrappedHandler = x => {
//		//			var context = x.Context as TDbContext;
//		//			if (context == null)
//		//				return;
//		//			handler(factory(x));
//		//		};
//		//		handlers.Add(wrappedHandler);
//		//		wrappers.Add(new Wrapper<T1, T2>(handler, wrappedHandler));
//		//	}
//		//}

//		//private static void Remove<T1, T2>(List<Wrapper<T1, T2>> wrappers, List<Action<T1>> handlers, Action<T2> handler) where T1 : class where T2 : class {
//		//	if (handler == null)
//		//		return;
//		//	lock (handlers) {
//		//		var index = wrappers.FindLastIndex(x => x.Outer == handler);
//		//		if (index < 0)
//		//			return;
//		//		var wrapper = wrappers[index];
//		//		wrappers.RemoveAt(index);
//		//		index = handlers.LastIndexOf(wrapper.Inner);
//		//		handlers.RemoveAt(index);
//		//	}
//		//}

//		private static void Add<T1, T2>(ref ImmutableArray<Handler<T1, T2>> wrappedHandlers, ref ImmutableArray<Action<T1>> handlers, Action<T2> handler, Func<T1, T2> factory) where T1 : class, IEntry<TTriggerable> where T2 : class {
//			if (handler == null)
//				return;

//			Action<T1> wrappedHandler = x => {
//				if (x.Context is TDbContext)
//					handler(factory(x));
//			};

//			ImmutableArray<Action<T1>> initialHandlers, computedHandlers;
//			do {
//				initialHandlers = ImmutableInterlocked.InterlockedCompareExchange(ref handlers, ImmutableArray<Action<T1>>.Empty, ImmutableArray<Action<T1>>.Empty);
//				computedHandlers = initialHandlers.Add(wrappedHandler);
//			}
//			while (initialHandlers != ImmutableInterlocked.InterlockedCompareExchange(ref handlers, computedHandlers, initialHandlers));

//			ImmutableArray<Handler<T1, T2>> initialWrappedHandlers, computedWrappedHandlers;
//			do {
//				initialWrappedHandlers = ImmutableInterlocked.InterlockedCompareExchange(ref wrappedHandlers, ImmutableArray<Handler<T1, T2>>.Empty, ImmutableArray<Handler<T1, T2>>.Empty);
//				computedWrappedHandlers = initialWrappedHandlers.Add(new Handler<T1, T2>(handler, wrappedHandler));
//			}
//			while (initialWrappedHandlers != ImmutableInterlocked.InterlockedCompareExchange(ref wrappedHandlers, computedWrappedHandlers, initialWrappedHandlers));
//		}

//		//private class OuterComparer<T1, T2> : IEqualityComparer<Wrapper<T1, T2>> {
//		//	public Boolean Equals(Wrapper<T1, T2> x, Wrapper<T1, T2> y) => x.Outer == y.Outer;
//		//	public Int32 GetHashCode(Wrapper<T1, T2> obj) => obj.Outer.GetHashCode();
//		//}

//		private static void Remove<T1, T2>(ref ImmutableArray<Handler<T1, T2>> wrappedHandlers, ref ImmutableArray<Action<T1>> handlers, Action<T2> handler) where T1 : class where T2 : class {
//			if (handler == null)
//				return;

//			// TODO: check order of removals for race

//			Handler<T1, T2> removedWrapper;
//			ImmutableArray<Handler<T1, T2>> initialWrappedHandlers, computedWrappedHandlers;
//			do {
//				initialWrappedHandlers = ImmutableInterlocked.InterlockedCompareExchange(ref wrappedHandlers, ImmutableArray<Handler<T1, T2>>.Empty, ImmutableArray<Handler<T1, T2>>.Empty);
//				var index = initialWrappedHandlers.LastIndexOf(new Handler<T1, T2>(handler, null), 0, initialWrappedHandlers.Length);
//				if (index < 0)
//					return;
//				removedWrapper = initialWrappedHandlers[index];
//				computedWrappedHandlers = initialWrappedHandlers.RemoveAt(index);
//			}
//			while (initialWrappedHandlers != ImmutableInterlocked.InterlockedCompareExchange(ref wrappedHandlers, computedWrappedHandlers, initialWrappedHandlers));

//			ImmutableArray<Action<T1>> initialHandlers, computedHandlers;
//			do {
//				initialHandlers = ImmutableInterlocked.InterlockedCompareExchange(ref handlers, ImmutableArray<Action<T1>>.Empty, ImmutableArray<Action<T1>>.Empty);
//				var index = initialHandlers.LastIndexOf(removedWrapper.Inner);
//				if (index < 0)
//					throw new Exception("Wrapped handler not found. There most likely is a race condition in EntityFramework.Triggers event handling code.");
//				computedHandlers = initialHandlers.RemoveAt(index);
//			}
//			while (initialHandlers != ImmutableInterlocked.InterlockedCompareExchange(ref handlers, computedHandlers, initialHandlers));
//		}
//		#endregion
//		#region Entry implementations
//		private class Entry : IEntry<TTriggerable, TDbContext> {
//			protected Entry(IEntry<TTriggerable, DbContext> entry) {
//				Entity = entry.Entity;
//				Context = (TDbContext) entry.Context;
//			}
//			public TTriggerable Entity { get; }
//			public TDbContext Context { get; }
//		}

//		private class AfterEntry : Entry, IAfterEntry<TTriggerable, TDbContext> {
//			private AfterEntry(IEntry<TTriggerable, DbContext> entry) : base(entry) {}
//			public static IAfterEntry<TTriggerable, TDbContext> Create(IAfterEntry<TTriggerable> entry) => new AfterEntry(entry);
//		}

//		private abstract class ChangeEntry : Entry, IChangeEntry<TTriggerable, TDbContext> {
//			protected ChangeEntry(IEntry<TTriggerable, DbContext> entry) : base(entry) {
//				original = new Lazy<TTriggerable>(() => Context.GetOriginal(Entity));
//			}

//			private readonly Lazy<TTriggerable> original;
//			public TTriggerable Original => original.Value;
//		}

//		private class AfterChangeEntry : ChangeEntry, IAfterChangeEntry<TTriggerable, TDbContext> {
//			private AfterChangeEntry(IEntry<TTriggerable, DbContext> entry) : base(entry) {}
//			public static IAfterChangeEntry<TTriggerable, TDbContext> Create(IAfterChangeEntry<TTriggerable> entry) => new AfterChangeEntry(entry);
//		}

//		private class FailedEntry : Entry, IFailedEntry<TTriggerable, TDbContext> {
//			private FailedEntry(IFailedEntry<TTriggerable, DbContext> entry) : base(entry) { Exception = entry.Exception; }
//			public Exception Exception { get; }
//			public static IFailedEntry<TTriggerable, TDbContext> Create(IFailedEntry<TTriggerable> entry) => new FailedEntry(entry);
//		}

//		private class ChangeFailedEntry : ChangeEntry, IChangeFailedEntry<TTriggerable, TDbContext> {
//			private ChangeFailedEntry(IChangeFailedEntry<TTriggerable> entry) : base(entry) { Exception = entry.Exception; }
//			public Exception Exception { get; }
//			public static IChangeFailedEntry<TTriggerable, TDbContext> Create(IChangeFailedEntry<TTriggerable> entry) => new ChangeFailedEntry(entry);
//		}

//		private class InsertingEntry : Entry, IBeforeEntry<TTriggerable, TDbContext> {
//			private InsertingEntry(IBeforeEntry<TTriggerable> entry) : base(entry) {}
//			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
//			public static IBeforeEntry<TTriggerable, TDbContext> Create(IBeforeEntry<TTriggerable> entry) => new InsertingEntry(entry);
//		}

//		private class UpdatingEntry : ChangeEntry, IBeforeChangeEntry<TTriggerable, TDbContext> {
//			private UpdatingEntry(IEntry<TTriggerable, DbContext> entry) : base(entry) {}
//			public void Cancel() => Context.Entry(Entity).State = EntityState.Unchanged;
//			public static IBeforeChangeEntry<TTriggerable, TDbContext> Create(IBeforeChangeEntry<TTriggerable> entry) => new UpdatingEntry(entry);
//		}

//		private class DeletingEntry : ChangeEntry, IBeforeChangeEntry<TTriggerable, TDbContext> {
//			private DeletingEntry(IEntry<TTriggerable, DbContext> entry) : base(entry) {}
//			public void Cancel() => Context.Entry(Entity).State = EntityState.Modified;
//			public static IBeforeChangeEntry<TTriggerable, TDbContext> Create(IBeforeChangeEntry<TTriggerable> entry) => new DeletingEntry(entry);
//		}
//		#endregion
//		#region Instance events
//		// We need to keep the wrappers too for comparing against on removal of a wrapped event handler
//		//private ImmutableArray<Handler<IBeforeEntry      <TTriggerable>, IBeforeEntry      <TTriggerable, TDbContext>>> insertingWrappers    = ImmutableArray<Handler<IBeforeEntry      <TTriggerable>, IBeforeEntry      <TTriggerable, TDbContext>>>.Empty;
//		//private ImmutableArray<Handler<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>> updatingWrappers     = ImmutableArray<Handler<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>>.Empty;
//		//private ImmutableArray<Handler<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>> deletingWrappers     = ImmutableArray<Handler<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>>.Empty;
//		//private ImmutableArray<Handler<IFailedEntry      <TTriggerable>, IFailedEntry      <TTriggerable, TDbContext>>> insertFailedWrappers = ImmutableArray<Handler<IFailedEntry      <TTriggerable>, IFailedEntry      <TTriggerable, TDbContext>>>.Empty;
//		//private ImmutableArray<Handler<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>> updateFailedWrappers = ImmutableArray<Handler<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>>.Empty;
//		//private ImmutableArray<Handler<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>> deleteFailedWrappers = ImmutableArray<Handler<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>>.Empty;
//		//private ImmutableArray<Handler<IAfterEntry       <TTriggerable>, IAfterEntry       <TTriggerable, TDbContext>>> insertedWrappers     = ImmutableArray<Handler<IAfterEntry       <TTriggerable>, IAfterEntry       <TTriggerable, TDbContext>>>.Empty;
//		//private ImmutableArray<Handler<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>> updatedWrappers      = ImmutableArray<Handler<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>>.Empty;
//		//private ImmutableArray<Handler<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>> deletedWrappers      = ImmutableArray<Handler<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>>.Empty;

//		event Action<IBeforeEntry      <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Inserting    { add { Add(ref triggers.inserting  , value, InsertingEntry   .Create); } remove { Remove(ref insertingWrappers   , ref triggers.inserting   , value); } }
//		event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Updating     { add { Add(ref updatingWrappers    , ref triggers.updating    , value, UpdatingEntry    .Create); } remove { Remove(ref updatingWrappers    , ref triggers.updating    , value); } }
//		event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Deleting     { add { Add(ref deletingWrappers    , ref triggers.deleting    , value, DeletingEntry    .Create); } remove { Remove(ref deletingWrappers    , ref triggers.deleting    , value); } }
//		event Action<IFailedEntry      <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.InsertFailed { add { Add(ref insertFailedWrappers, ref triggers.insertFailed, value, FailedEntry      .Create); } remove { Remove(ref insertFailedWrappers, ref triggers.insertFailed, value); } }
//		event Action<IChangeFailedEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.UpdateFailed { add { Add(ref updateFailedWrappers, ref triggers.updateFailed, value, ChangeFailedEntry.Create); } remove { Remove(ref updateFailedWrappers, ref triggers.updateFailed, value); } }
//		event Action<IChangeFailedEntry<TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.DeleteFailed { add { Add(ref deleteFailedWrappers, ref triggers.deleteFailed, value, ChangeFailedEntry.Create); } remove { Remove(ref deleteFailedWrappers, ref triggers.deleteFailed, value); } }
//		event Action<IAfterEntry       <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Inserted     { add { Add(ref insertedWrappers    , ref triggers.inserted    , value, AfterEntry       .Create); } remove { Remove(ref insertedWrappers    , ref triggers.inserted    , value); } }
//		event Action<IAfterChangeEntry <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Updated      { add { Add(ref updatedWrappers     , ref triggers.updated     , value, AfterChangeEntry .Create); } remove { Remove(ref updatedWrappers     , ref triggers.updated     , value); } }
//		event Action<IAfterChangeEntry <TTriggerable, TDbContext>> ITriggers<TTriggerable, TDbContext>.Deleted      { add { Add(ref deletedWrappers     , ref triggers.deleted     , value, AfterChangeEntry .Create); } remove { Remove(ref deletedWrappers     , ref triggers.deleted     , value); } }

//		void ITriggers.OnBeforeInsert(ITriggerable t, DbContext dbc)               => ((ITriggers) triggers).OnBeforeInsert(t, dbc);
//		void ITriggers.OnBeforeUpdate(ITriggerable t, DbContext dbc)               => ((ITriggers) triggers).OnBeforeUpdate(t, dbc);
//		void ITriggers.OnBeforeDelete(ITriggerable t, DbContext dbc)               => ((ITriggers) triggers).OnBeforeDelete(t, dbc);
//		void ITriggers.OnInsertFailed(ITriggerable t, DbContext dbc, Exception ex) => ((ITriggers) triggers).OnInsertFailed(t, dbc, ex);
//		void ITriggers.OnUpdateFailed(ITriggerable t, DbContext dbc, Exception ex) => ((ITriggers) triggers).OnUpdateFailed(t, dbc, ex);
//		void ITriggers.OnDeleteFailed(ITriggerable t, DbContext dbc, Exception ex) => ((ITriggers) triggers).OnDeleteFailed(t, dbc, ex);
//		void ITriggers.OnAfterInsert (ITriggerable t, DbContext dbc)               => ((ITriggers) triggers).OnAfterInsert (t, dbc);
//		void ITriggers.OnAfterUpdate (ITriggerable t, DbContext dbc)               => ((ITriggers) triggers).OnAfterUpdate (t, dbc);
//		void ITriggers.OnAfterDelete (ITriggerable t, DbContext dbc)               => ((ITriggers) triggers).OnAfterDelete (t, dbc);
//		#endregion
//		#region Static events
//		// We need to keep the wrappers too for comparing against on removal of a wrapped event handler
//		private static ImmutableArray<Wrapper<IBeforeEntry      <TTriggerable>, IBeforeEntry      <TTriggerable, TDbContext>>> staticInsertingWrappers    = ImmutableArray<Wrapper<IBeforeEntry      <TTriggerable>, IBeforeEntry      <TTriggerable, TDbContext>>>.Empty;
//		private static ImmutableArray<Wrapper<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>> staticUpdatingWrappers     = ImmutableArray<Wrapper<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>>.Empty;
//		private static ImmutableArray<Wrapper<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>> staticDeletingWrappers     = ImmutableArray<Wrapper<IBeforeChangeEntry<TTriggerable>, IBeforeChangeEntry<TTriggerable, TDbContext>>>.Empty;
//		private static ImmutableArray<Wrapper<IFailedEntry      <TTriggerable>, IFailedEntry      <TTriggerable, TDbContext>>> staticInsertFailedWrappers = ImmutableArray<Wrapper<IFailedEntry      <TTriggerable>, IFailedEntry      <TTriggerable, TDbContext>>>.Empty;
//		private static ImmutableArray<Wrapper<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>> staticUpdateFailedWrappers = ImmutableArray<Wrapper<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>>.Empty;
//		private static ImmutableArray<Wrapper<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>> staticDeleteFailedWrappers = ImmutableArray<Wrapper<IChangeFailedEntry<TTriggerable>, IChangeFailedEntry<TTriggerable, TDbContext>>>.Empty;
//		private static ImmutableArray<Wrapper<IAfterEntry       <TTriggerable>, IAfterEntry       <TTriggerable, TDbContext>>> staticInsertedWrappers     = ImmutableArray<Wrapper<IAfterEntry       <TTriggerable>, IAfterEntry       <TTriggerable, TDbContext>>>.Empty;
//		private static ImmutableArray<Wrapper<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>> staticUpdatedWrappers      = ImmutableArray<Wrapper<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>>.Empty;
//		private static ImmutableArray<Wrapper<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>> staticDeletedWrappers      = ImmutableArray<Wrapper<IAfterChangeEntry <TTriggerable>, IAfterChangeEntry <TTriggerable, TDbContext>>>.Empty;

//		public static event Action<IBeforeEntry      <TTriggerable, TDbContext>> Inserting    { add { Add(ref staticInsertingWrappers   , ref Triggers<TTriggerable>.staticInserting   , value, InsertingEntry   .Create); } remove { Remove(ref staticInsertingWrappers   , ref Triggers<TTriggerable>.staticInserting   , value); } }
//		public static event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> Updating     { add { Add(ref staticUpdatingWrappers    , ref Triggers<TTriggerable>.staticUpdating    , value, UpdatingEntry    .Create); } remove { Remove(ref staticUpdatingWrappers    , ref Triggers<TTriggerable>.staticUpdating    , value); } }
//		public static event Action<IBeforeChangeEntry<TTriggerable, TDbContext>> Deleting     { add { Add(ref staticDeletingWrappers    , ref Triggers<TTriggerable>.staticDeleting    , value, DeletingEntry    .Create); } remove { Remove(ref staticDeletingWrappers    , ref Triggers<TTriggerable>.staticDeleting    , value); } }
//		public static event Action<IFailedEntry      <TTriggerable, TDbContext>> InsertFailed { add { Add(ref staticInsertFailedWrappers, ref Triggers<TTriggerable>.staticInsertFailed, value, FailedEntry      .Create); } remove { Remove(ref staticInsertFailedWrappers, ref Triggers<TTriggerable>.staticInsertFailed, value); } }
//		public static event Action<IChangeFailedEntry<TTriggerable, TDbContext>> UpdateFailed { add { Add(ref staticUpdateFailedWrappers, ref Triggers<TTriggerable>.staticUpdateFailed, value, ChangeFailedEntry.Create); } remove { Remove(ref staticUpdateFailedWrappers, ref Triggers<TTriggerable>.staticUpdateFailed, value); } }
//		public static event Action<IChangeFailedEntry<TTriggerable, TDbContext>> DeleteFailed { add { Add(ref staticDeleteFailedWrappers, ref Triggers<TTriggerable>.staticDeleteFailed, value, ChangeFailedEntry.Create); } remove { Remove(ref staticDeleteFailedWrappers, ref Triggers<TTriggerable>.staticDeleteFailed, value); } }
//		public static event Action<IAfterEntry       <TTriggerable, TDbContext>> Inserted     { add { Add(ref staticInsertedWrappers    , ref Triggers<TTriggerable>.staticInserted    , value, AfterEntry       .Create); } remove { Remove(ref staticInsertedWrappers    , ref Triggers<TTriggerable>.staticInserted    , value); } }
//		public static event Action<IAfterChangeEntry <TTriggerable, TDbContext>> Updated      { add { Add(ref staticUpdatedWrappers     , ref Triggers<TTriggerable>.staticUpdated     , value, AfterChangeEntry .Create); } remove { Remove(ref staticUpdatedWrappers     , ref Triggers<TTriggerable>.staticUpdated     , value); } }
//		public static event Action<IAfterChangeEntry <TTriggerable, TDbContext>> Deleted      { add { Add(ref staticDeletedWrappers     , ref Triggers<TTriggerable>.staticDeleted     , value, AfterChangeEntry .Create); } remove { Remove(ref staticDeletedWrappers     , ref Triggers<TTriggerable>.staticDeleted     , value); } }
//		#endregion
//	}
//}
