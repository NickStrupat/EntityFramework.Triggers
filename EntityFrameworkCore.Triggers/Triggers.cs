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
		private abstract class Entry : IEntry<TEntity, TDbContext> {
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
