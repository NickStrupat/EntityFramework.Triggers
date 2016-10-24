using System;
using System.Collections.Immutable;
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
		private static ImmutableArray<Action<IBeforeEntry      <TEntity, TDbContext>>> inserting    = ImmutableArray<Action<IBeforeEntry      <TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IBeforeChangeEntry<TEntity, TDbContext>>> updating     = ImmutableArray<Action<IBeforeChangeEntry<TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IBeforeChangeEntry<TEntity, TDbContext>>> deleting     = ImmutableArray<Action<IBeforeChangeEntry<TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IFailedEntry      <TEntity, TDbContext>>> insertFailed = ImmutableArray<Action<IFailedEntry      <TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IChangeFailedEntry<TEntity, TDbContext>>> updateFailed = ImmutableArray<Action<IChangeFailedEntry<TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IChangeFailedEntry<TEntity, TDbContext>>> deleteFailed = ImmutableArray<Action<IChangeFailedEntry<TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IAfterEntry       <TEntity, TDbContext>>> inserted     = ImmutableArray<Action<IAfterEntry       <TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IAfterChangeEntry <TEntity, TDbContext>>> updated      = ImmutableArray<Action<IAfterChangeEntry <TEntity, TDbContext>>>.Empty;
		private static ImmutableArray<Action<IAfterChangeEntry <TEntity, TDbContext>>> deleted      = ImmutableArray<Action<IAfterChangeEntry <TEntity, TDbContext>>>.Empty;

		public static event Action<IBeforeEntry      <TEntity, TDbContext>> Inserting    { add { Add(ref inserting   , value); } remove { Remove(ref inserting   , value); } }
		public static event Action<IBeforeChangeEntry<TEntity, TDbContext>> Updating     { add { Add(ref updating    , value); } remove { Remove(ref updating    , value); } }
		public static event Action<IBeforeChangeEntry<TEntity, TDbContext>> Deleting     { add { Add(ref deleting    , value); } remove { Remove(ref deleting    , value); } }
		public static event Action<IFailedEntry      <TEntity, TDbContext>> InsertFailed { add { Add(ref insertFailed, value); } remove { Remove(ref insertFailed, value); } }
		public static event Action<IChangeFailedEntry<TEntity, TDbContext>> UpdateFailed { add { Add(ref updateFailed, value); } remove { Remove(ref updateFailed, value); } }
		public static event Action<IChangeFailedEntry<TEntity, TDbContext>> DeleteFailed { add { Add(ref deleteFailed, value); } remove { Remove(ref deleteFailed, value); } }
		public static event Action<IAfterEntry       <TEntity, TDbContext>> Inserted     { add { Add(ref inserted    , value); } remove { Remove(ref inserted    , value); } }
		public static event Action<IAfterChangeEntry <TEntity, TDbContext>> Updated      { add { Add(ref updated     , value); } remove { Remove(ref updated     , value); } }
		public static event Action<IAfterChangeEntry <TEntity, TDbContext>> Deleted      { add { Add(ref deleted     , value); } remove { Remove(ref deleted     , value); } }

		internal static void RaiseBeforeInsert(TEntity entity, TDbContext dbc)               => Raise(ref inserting   , new InsertingEntry    { Entity = entity, Context = dbc });
		internal static void RaiseBeforeUpdate(TEntity entity, TDbContext dbc)               => Raise(ref updating    , new UpdatingEntry     { Entity = entity, Context = dbc });
		internal static void RaiseBeforeDelete(TEntity entity, TDbContext dbc)               => Raise(ref deleting    , new DeletingEntry     { Entity = entity, Context = dbc });
		internal static void RaiseInsertFailed(TEntity entity, TDbContext dbc, Exception ex) => Raise(ref insertFailed, new FailedEntry       { Entity = entity, Context = dbc, Exception = ex });
		internal static void RaiseUpdateFailed(TEntity entity, TDbContext dbc, Exception ex) => Raise(ref updateFailed, new ChangeFailedEntry { Entity = entity, Context = dbc, Exception = ex });
		internal static void RaiseDeleteFailed(TEntity entity, TDbContext dbc, Exception ex) => Raise(ref deleteFailed, new ChangeFailedEntry { Entity = entity, Context = dbc, Exception = ex });
		internal static void RaiseAfterInsert (TEntity entity, TDbContext dbc)               => Raise(ref inserted    , new AfterEntry        { Entity = entity, Context = dbc });
		internal static void RaiseAfterUpdate (TEntity entity, TDbContext dbc)               => Raise(ref updated     , new AfterChangeEntry  { Entity = entity, Context = dbc });
		internal static void RaiseAfterDelete (TEntity entity, TDbContext dbc)               => Raise(ref deleted     , new AfterChangeEntry  { Entity = entity, Context = dbc });
		#endregion
		#region Entry implementations
		private abstract class Entry : IEntry<TEntity, TDbContext> {
			public TEntity Entity { get; internal set; }
			public TDbContext Context { get; internal set; }
			DbContext IEntry<TEntity>.Context => Context;
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
			public void Cancel() => Context.Entry(Entity).State = EntityState.Detached;
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
