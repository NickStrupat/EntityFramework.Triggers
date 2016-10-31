using System;
using CoContra;
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
		private static readonly CovariantAction<IBeforeEntry      <TEntity, TDbContext>> inserting    = new CovariantAction<IBeforeEntry      <TEntity, TDbContext>>();
		private static readonly CovariantAction<IBeforeChangeEntry<TEntity, TDbContext>> updating     = new CovariantAction<IBeforeChangeEntry<TEntity, TDbContext>>();
		private static readonly CovariantAction<IBeforeChangeEntry<TEntity, TDbContext>> deleting     = new CovariantAction<IBeforeChangeEntry<TEntity, TDbContext>>();
		private static readonly CovariantAction<IFailedEntry      <TEntity, TDbContext>> insertFailed = new CovariantAction<IFailedEntry      <TEntity, TDbContext>>();
		private static readonly CovariantAction<IChangeFailedEntry<TEntity, TDbContext>> updateFailed = new CovariantAction<IChangeFailedEntry<TEntity, TDbContext>>();
		private static readonly CovariantAction<IChangeFailedEntry<TEntity, TDbContext>> deleteFailed = new CovariantAction<IChangeFailedEntry<TEntity, TDbContext>>();
		private static readonly CovariantAction<IAfterEntry       <TEntity, TDbContext>> inserted     = new CovariantAction<IAfterEntry       <TEntity, TDbContext>>();
		private static readonly CovariantAction<IAfterChangeEntry <TEntity, TDbContext>> updated      = new CovariantAction<IAfterChangeEntry <TEntity, TDbContext>>();
		private static readonly CovariantAction<IAfterChangeEntry <TEntity, TDbContext>> deleted      = new CovariantAction<IAfterChangeEntry <TEntity, TDbContext>>();

		public static event Action<IBeforeEntry      <TEntity, TDbContext>> Inserting    { add { inserting   .Add(value); } remove { inserting   .Remove(value); } }
		public static event Action<IBeforeChangeEntry<TEntity, TDbContext>> Updating     { add { updating    .Add(value); } remove { updating    .Remove(value); } }
		public static event Action<IBeforeChangeEntry<TEntity, TDbContext>> Deleting     { add { deleting    .Add(value); } remove { deleting    .Remove(value); } }
		public static event Action<IFailedEntry      <TEntity, TDbContext>> InsertFailed { add { insertFailed.Add(value); } remove { insertFailed.Remove(value); } }
		public static event Action<IChangeFailedEntry<TEntity, TDbContext>> UpdateFailed { add { updateFailed.Add(value); } remove { updateFailed.Remove(value); } }
		public static event Action<IChangeFailedEntry<TEntity, TDbContext>> DeleteFailed { add { deleteFailed.Add(value); } remove { deleteFailed.Remove(value); } }
		public static event Action<IAfterEntry       <TEntity, TDbContext>> Inserted     { add { inserted    .Add(value); } remove { inserted    .Remove(value); } }
		public static event Action<IAfterChangeEntry <TEntity, TDbContext>> Updated      { add { updated     .Add(value); } remove { updated     .Remove(value); } }
		public static event Action<IAfterChangeEntry <TEntity, TDbContext>> Deleted      { add { deleted     .Add(value); } remove { deleted     .Remove(value); } }

		internal static void RaiseBeforeInsert(IBeforeEntry      <TEntity, TDbContext> entry) => inserting   .Invoke(entry);
		internal static void RaiseBeforeUpdate(IBeforeChangeEntry<TEntity, TDbContext> entry) => updating    .Invoke(entry);
		internal static void RaiseBeforeDelete(IBeforeChangeEntry<TEntity, TDbContext> entry) => deleting    .Invoke(entry);
		internal static void RaiseInsertFailed(IFailedEntry      <TEntity, TDbContext> entry) => insertFailed.Invoke(entry);
		internal static void RaiseUpdateFailed(IChangeFailedEntry<TEntity, TDbContext> entry) => updateFailed.Invoke(entry);
		internal static void RaiseDeleteFailed(IChangeFailedEntry<TEntity, TDbContext> entry) => deleteFailed.Invoke(entry);
		internal static void RaiseAfterInsert (IAfterEntry       <TEntity, TDbContext> entry) => inserted    .Invoke(entry);
		internal static void RaiseAfterUpdate (IAfterChangeEntry <TEntity, TDbContext> entry) => updated     .Invoke(entry);
		internal static void RaiseAfterDelete (IAfterChangeEntry <TEntity, TDbContext> entry) => deleted     .Invoke(entry);
	}
}
