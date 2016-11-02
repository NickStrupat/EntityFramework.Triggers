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
		public static event Action<IInsertingEntry   <TEntity, DbContext>> Inserting    { add { Triggers<TEntity, DbContext>.Inserting    += value; } remove { Triggers<TEntity, DbContext>.Inserting    -= value; } }
		public static event Action<IUpdatingEntry    <TEntity, DbContext>> Updating     { add { Triggers<TEntity, DbContext>.Updating     += value; } remove { Triggers<TEntity, DbContext>.Updating     -= value; } }
		public static event Action<IDeletingEntry    <TEntity, DbContext>> Deleting     { add { Triggers<TEntity, DbContext>.Deleting     += value; } remove { Triggers<TEntity, DbContext>.Deleting     -= value; } }
		public static event Action<IInsertFailedEntry<TEntity, DbContext>> InsertFailed { add { Triggers<TEntity, DbContext>.InsertFailed += value; } remove { Triggers<TEntity, DbContext>.InsertFailed -= value; } }
		public static event Action<IUpdateFailedEntry<TEntity, DbContext>> UpdateFailed { add { Triggers<TEntity, DbContext>.UpdateFailed += value; } remove { Triggers<TEntity, DbContext>.UpdateFailed -= value; } }
		public static event Action<IDeleteFailedEntry<TEntity, DbContext>> DeleteFailed { add { Triggers<TEntity, DbContext>.DeleteFailed += value; } remove { Triggers<TEntity, DbContext>.DeleteFailed -= value; } }
		public static event Action<IInsertedEntry    <TEntity, DbContext>> Inserted     { add { Triggers<TEntity, DbContext>.Inserted     += value; } remove { Triggers<TEntity, DbContext>.Inserted     -= value; } }
		public static event Action<IUpdatedEntry     <TEntity, DbContext>> Updated      { add { Triggers<TEntity, DbContext>.Updated      += value; } remove { Triggers<TEntity, DbContext>.Updated      -= value; } }
		public static event Action<IDeletedEntry     <TEntity, DbContext>> Deleted      { add { Triggers<TEntity, DbContext>.Deleted      += value; } remove { Triggers<TEntity, DbContext>.Deleted      -= value; } }
	}

	public static class Triggers<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {
		private static CovariantAction<IInsertingEntry   <TEntity, TDbContext>> inserting    = new CovariantAction<IInsertingEntry   <TEntity, TDbContext>>();
		private static CovariantAction<IUpdatingEntry    <TEntity, TDbContext>> updating     = new CovariantAction<IUpdatingEntry    <TEntity, TDbContext>>();
		private static CovariantAction<IDeletingEntry    <TEntity, TDbContext>> deleting     = new CovariantAction<IDeletingEntry    <TEntity, TDbContext>>();
		private static CovariantAction<IInsertFailedEntry<TEntity, TDbContext>> insertFailed = new CovariantAction<IInsertFailedEntry<TEntity, TDbContext>>();
		private static CovariantAction<IUpdateFailedEntry<TEntity, TDbContext>> updateFailed = new CovariantAction<IUpdateFailedEntry<TEntity, TDbContext>>();
		private static CovariantAction<IDeleteFailedEntry<TEntity, TDbContext>> deleteFailed = new CovariantAction<IDeleteFailedEntry<TEntity, TDbContext>>();
		private static CovariantAction<IInsertedEntry    <TEntity, TDbContext>> inserted     = new CovariantAction<IInsertedEntry    <TEntity, TDbContext>>();
		private static CovariantAction<IUpdatedEntry     <TEntity, TDbContext>> updated      = new CovariantAction<IUpdatedEntry     <TEntity, TDbContext>>();
		private static CovariantAction<IDeletedEntry     <TEntity, TDbContext>> deleted      = new CovariantAction<IDeletedEntry     <TEntity, TDbContext>>();

		public static event Action<IInsertingEntry   <TEntity, TDbContext>> Inserting    { add { inserting   .Add(value); } remove { inserting   .Remove(value); } }
		public static event Action<IUpdatingEntry    <TEntity, TDbContext>> Updating     { add { updating    .Add(value); } remove { updating    .Remove(value); } }
		public static event Action<IDeletingEntry    <TEntity, TDbContext>> Deleting     { add { deleting    .Add(value); } remove { deleting    .Remove(value); } }
		public static event Action<IInsertFailedEntry<TEntity, TDbContext>> InsertFailed { add { insertFailed.Add(value); } remove { insertFailed.Remove(value); } }
		public static event Action<IUpdateFailedEntry<TEntity, TDbContext>> UpdateFailed { add { updateFailed.Add(value); } remove { updateFailed.Remove(value); } }
		public static event Action<IDeleteFailedEntry<TEntity, TDbContext>> DeleteFailed { add { deleteFailed.Add(value); } remove { deleteFailed.Remove(value); } }
		public static event Action<IInsertedEntry    <TEntity, TDbContext>> Inserted     { add { inserted    .Add(value); } remove { inserted    .Remove(value); } }
		public static event Action<IUpdatedEntry     <TEntity, TDbContext>> Updated      { add { updated     .Add(value); } remove { updated     .Remove(value); } }
		public static event Action<IDeletedEntry     <TEntity, TDbContext>> Deleted      { add { deleted     .Add(value); } remove { deleted     .Remove(value); } }

		internal static void RaiseBeforeInsert(IInsertingEntry   <TEntity, TDbContext> entry) => inserting   .Invoke(entry);
		internal static void RaiseBeforeUpdate(IUpdatingEntry    <TEntity, TDbContext> entry) => updating    .Invoke(entry);
		internal static void RaiseBeforeDelete(IDeletingEntry    <TEntity, TDbContext> entry) => deleting    .Invoke(entry);
		internal static void RaiseInsertFailed(IInsertFailedEntry<TEntity, TDbContext> entry) => insertFailed.Invoke(entry);
		internal static void RaiseUpdateFailed(IUpdateFailedEntry<TEntity, TDbContext> entry) => updateFailed.Invoke(entry);
		internal static void RaiseDeleteFailed(IDeleteFailedEntry<TEntity, TDbContext> entry) => deleteFailed.Invoke(entry);
		internal static void RaiseAfterInsert (IInsertedEntry    <TEntity, TDbContext> entry) => inserted    .Invoke(entry);
		internal static void RaiseAfterUpdate (IUpdatedEntry     <TEntity, TDbContext> entry) => updated     .Invoke(entry);
		internal static void RaiseAfterDelete (IDeletedEntry     <TEntity, TDbContext> entry) => deleted     .Invoke(entry);
	}
}
