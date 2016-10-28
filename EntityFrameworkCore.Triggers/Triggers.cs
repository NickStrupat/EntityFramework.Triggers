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

		internal static void RaiseBeforeInsert(IBeforeEntry      <TEntity, TDbContext> entry) => Raise(ref inserting   , entry);
		internal static void RaiseBeforeUpdate(IBeforeChangeEntry<TEntity, TDbContext> entry) => Raise(ref updating    , entry);
		internal static void RaiseBeforeDelete(IBeforeChangeEntry<TEntity, TDbContext> entry) => Raise(ref deleting    , entry);
		internal static void RaiseInsertFailed(IFailedEntry      <TEntity, TDbContext> entry) => Raise(ref insertFailed, entry);
		internal static void RaiseUpdateFailed(IChangeFailedEntry<TEntity, TDbContext> entry) => Raise(ref updateFailed, entry);
		internal static void RaiseDeleteFailed(IChangeFailedEntry<TEntity, TDbContext> entry) => Raise(ref deleteFailed, entry);
		internal static void RaiseAfterInsert (IAfterEntry       <TEntity, TDbContext> entry) => Raise(ref inserted    , entry);
		internal static void RaiseAfterUpdate (IAfterChangeEntry <TEntity, TDbContext> entry) => Raise(ref updated     , entry);
		internal static void RaiseAfterDelete (IAfterChangeEntry <TEntity, TDbContext> entry) => Raise(ref deleted     , entry);
		#endregion
	}
}
