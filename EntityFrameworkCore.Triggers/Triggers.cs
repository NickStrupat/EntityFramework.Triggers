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
		/// <summary>The triggers fired before an entity is inserted</summary>
		public static event Action<IInsertingEntry   <TEntity, DbContext>> Inserting    { add { Triggers<TEntity, DbContext>.Inserting    += value; } remove { Triggers<TEntity, DbContext>.Inserting    -= value; } }
		/// <summary>The triggers fired before an entity is updated</summary>
		public static event Action<IUpdatingEntry    <TEntity, DbContext>> Updating     { add { Triggers<TEntity, DbContext>.Updating     += value; } remove { Triggers<TEntity, DbContext>.Updating     -= value; } }
		/// <summary>The triggers fired before an entity is deleted</summary>
		public static event Action<IDeletingEntry    <TEntity, DbContext>> Deleting     { add { Triggers<TEntity, DbContext>.Deleting     += value; } remove { Triggers<TEntity, DbContext>.Deleting     -= value; } }
		/// <summary>The triggers fired when an insertion throws an exception</summary>
		public static event Action<IInsertFailedEntry<TEntity, DbContext>> InsertFailed { add { Triggers<TEntity, DbContext>.InsertFailed += value; } remove { Triggers<TEntity, DbContext>.InsertFailed -= value; } }
		/// <summary>The triggers fired when an update throws an exception</summary>
		public static event Action<IUpdateFailedEntry<TEntity, DbContext>> UpdateFailed { add { Triggers<TEntity, DbContext>.UpdateFailed += value; } remove { Triggers<TEntity, DbContext>.UpdateFailed -= value; } }
		/// <summary>The triggers fired when an deletion throws an exception</summary>
		public static event Action<IDeleteFailedEntry<TEntity, DbContext>> DeleteFailed { add { Triggers<TEntity, DbContext>.DeleteFailed += value; } remove { Triggers<TEntity, DbContext>.DeleteFailed -= value; } }
		/// <summary>The triggers fired after an entity is inserted</summary>
		public static event Action<IInsertedEntry    <TEntity, DbContext>> Inserted     { add { Triggers<TEntity, DbContext>.Inserted     += value; } remove { Triggers<TEntity, DbContext>.Inserted     -= value; } }
		/// <summary>The triggers fired after an entity is updated</summary>
		public static event Action<IUpdatedEntry     <TEntity, DbContext>> Updated      { add { Triggers<TEntity, DbContext>.Updated      += value; } remove { Triggers<TEntity, DbContext>.Updated      -= value; } }
		/// <summary>The triggers fired after an entity is deleted</summary>
		public static event Action<IDeletedEntry     <TEntity, DbContext>> Deleted      { add { Triggers<TEntity, DbContext>.Deleted      += value; } remove { Triggers<TEntity, DbContext>.Deleted      -= value; } }
	}

	public static class Triggers<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {
		private static readonly CovariantAction<IInsertingEntry   <TEntity, TDbContext>> inserting    = new CovariantAction<IInsertingEntry   <TEntity, TDbContext>>();
		private static readonly CovariantAction<IUpdatingEntry    <TEntity, TDbContext>> updating     = new CovariantAction<IUpdatingEntry    <TEntity, TDbContext>>();
		private static readonly CovariantAction<IDeletingEntry    <TEntity, TDbContext>> deleting     = new CovariantAction<IDeletingEntry    <TEntity, TDbContext>>();
		private static readonly CovariantAction<IInsertFailedEntry<TEntity, TDbContext>> insertFailed = new CovariantAction<IInsertFailedEntry<TEntity, TDbContext>>();
		private static readonly CovariantAction<IUpdateFailedEntry<TEntity, TDbContext>> updateFailed = new CovariantAction<IUpdateFailedEntry<TEntity, TDbContext>>();
		private static readonly CovariantAction<IDeleteFailedEntry<TEntity, TDbContext>> deleteFailed = new CovariantAction<IDeleteFailedEntry<TEntity, TDbContext>>();
		private static readonly CovariantAction<IInsertedEntry    <TEntity, TDbContext>> inserted     = new CovariantAction<IInsertedEntry    <TEntity, TDbContext>>();
		private static readonly CovariantAction<IUpdatedEntry     <TEntity, TDbContext>> updated      = new CovariantAction<IUpdatedEntry     <TEntity, TDbContext>>();
		private static readonly CovariantAction<IDeletedEntry     <TEntity, TDbContext>> deleted      = new CovariantAction<IDeletedEntry     <TEntity, TDbContext>>();

		/// <summary>The triggers fired before an entity is inserted</summary>
		public static event Action<IInsertingEntry   <TEntity, TDbContext>> Inserting    { add { inserting   .Add(value); } remove { inserting   .Remove(value); } }
		/// <summary>The triggers fired before an entity is updated</summary>
		public static event Action<IUpdatingEntry    <TEntity, TDbContext>> Updating     { add { updating    .Add(value); } remove { updating    .Remove(value); } }
		/// <summary>The triggers fired before an entity is deleted</summary>
		public static event Action<IDeletingEntry    <TEntity, TDbContext>> Deleting     { add { deleting    .Add(value); } remove { deleting    .Remove(value); } }
		/// <summary>The triggers fired when an insertion throws an exception</summary>
		public static event Action<IInsertFailedEntry<TEntity, TDbContext>> InsertFailed { add { insertFailed.Add(value); } remove { insertFailed.Remove(value); } }
		/// <summary>The triggers fired when an update throws an exception</summary>
		public static event Action<IUpdateFailedEntry<TEntity, TDbContext>> UpdateFailed { add { updateFailed.Add(value); } remove { updateFailed.Remove(value); } }
		/// <summary>The triggers fired when an deletion throws an exception</summary>
		public static event Action<IDeleteFailedEntry<TEntity, TDbContext>> DeleteFailed { add { deleteFailed.Add(value); } remove { deleteFailed.Remove(value); } }
		/// <summary>The triggers fired after an entity is inserted</summary>
		public static event Action<IInsertedEntry    <TEntity, TDbContext>> Inserted     { add { inserted    .Add(value); } remove { inserted    .Remove(value); } }
		/// <summary>The triggers fired after an entity is updated</summary>
		public static event Action<IUpdatedEntry     <TEntity, TDbContext>> Updated      { add { updated     .Add(value); } remove { updated     .Remove(value); } }
		/// <summary>The triggers fired after an entity is deleted</summary>
		public static event Action<IDeletedEntry     <TEntity, TDbContext>> Deleted      { add { deleted     .Add(value); } remove { deleted     .Remove(value); } }

		internal static void RaiseInserting   (IInsertingEntry   <TEntity, TDbContext> entry) => inserting   .Invoke(entry);
		internal static void RaiseUpdating    (IUpdatingEntry    <TEntity, TDbContext> entry) => updating    .Invoke(entry);
		internal static void RaiseDeleting    (IDeletingEntry    <TEntity, TDbContext> entry) => deleting    .Invoke(entry);
		internal static void RaiseInsertFailed(IInsertFailedEntry<TEntity, TDbContext> entry) => insertFailed.Invoke(entry);
		internal static void RaiseUpdateFailed(IUpdateFailedEntry<TEntity, TDbContext> entry) => updateFailed.Invoke(entry);
		internal static void RaiseDeleteFailed(IDeleteFailedEntry<TEntity, TDbContext> entry) => deleteFailed.Invoke(entry);
		internal static void RaiseInserted    (IInsertedEntry    <TEntity, TDbContext> entry) => inserted    .Invoke(entry);
		internal static void RaiseUpdated     (IUpdatedEntry     <TEntity, TDbContext> entry) => updated     .Invoke(entry);
		internal static void RaiseDeleted     (IDeletedEntry     <TEntity, TDbContext> entry) => deleted     .Invoke(entry);
	}
}
