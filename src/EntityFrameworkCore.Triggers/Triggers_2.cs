using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	public sealed class Triggers<TEntity, TDbContext>
	: ITriggers<TEntity, TDbContext>
	, IEquatable<ITriggers<TEntity, TDbContext>>
	where TEntity : class
	where TDbContext : DbContext
	{
		private readonly IInsertingTriggerEvent   <TEntity, TDbContext> inserting    = new InsertingTriggerEvent   <TEntity, TDbContext>();
		private readonly IInsertFailedTriggerEvent<TEntity, TDbContext> insertFailed = new InsertFailedTriggerEvent<TEntity, TDbContext>();
		private readonly IInsertedTriggerEvent    <TEntity, TDbContext> inserted     = new InsertedTriggerEvent    <TEntity, TDbContext>();
		private readonly IDeletingTriggerEvent    <TEntity, TDbContext> deleting     = new DeletingTriggerEvent    <TEntity, TDbContext>();
		private readonly IDeleteFailedTriggerEvent<TEntity, TDbContext> deleteFailed = new DeleteFailedTriggerEvent<TEntity, TDbContext>();
		private readonly IDeletedTriggerEvent     <TEntity, TDbContext> deleted      = new DeletedTriggerEvent     <TEntity, TDbContext>();
		private readonly IUpdatingTriggerEvent    <TEntity, TDbContext> updating     = new UpdatingTriggerEvent    <TEntity, TDbContext>();
		private readonly IUpdateFailedTriggerEvent<TEntity, TDbContext> updateFailed = new UpdateFailedTriggerEvent<TEntity, TDbContext>();
		private readonly IUpdatedTriggerEvent     <TEntity, TDbContext> updated      = new UpdatedTriggerEvent     <TEntity, TDbContext>();

		IInsertingTriggerEvent   <TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Inserting    => inserting   ;
		IInsertFailedTriggerEvent<TEntity, TDbContext> ITriggers<TEntity, TDbContext>.InsertFailed => insertFailed;
		IInsertedTriggerEvent    <TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Inserted     => inserted    ;
		IDeletingTriggerEvent    <TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Deleting     => deleting    ;
		IDeleteFailedTriggerEvent<TEntity, TDbContext> ITriggers<TEntity, TDbContext>.DeleteFailed => deleteFailed;
		IDeletedTriggerEvent     <TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Deleted      => deleted     ;
		IUpdatingTriggerEvent    <TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Updating     => updating    ;
		IUpdateFailedTriggerEvent<TEntity, TDbContext> ITriggers<TEntity, TDbContext>.UpdateFailed => updateFailed;
		IUpdatedTriggerEvent     <TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Updated      => updated     ;

		IInsertingTriggerEvent   <TEntity, DbContext> ITriggers<TEntity>.Inserting    => inserting   ;
		IInsertFailedTriggerEvent<TEntity, DbContext> ITriggers<TEntity>.InsertFailed => insertFailed;
		IInsertedTriggerEvent    <TEntity, DbContext> ITriggers<TEntity>.Inserted     => inserted    ;
		IDeletingTriggerEvent    <TEntity, DbContext> ITriggers<TEntity>.Deleting     => deleting    ;
		IDeleteFailedTriggerEvent<TEntity, DbContext> ITriggers<TEntity>.DeleteFailed => deleteFailed;
		IDeletedTriggerEvent     <TEntity, DbContext> ITriggers<TEntity>.Deleted      => deleted     ;
		IUpdatingTriggerEvent    <TEntity, DbContext> ITriggers<TEntity>.Updating     => updating    ;
		IUpdateFailedTriggerEvent<TEntity, DbContext> ITriggers<TEntity>.UpdateFailed => updateFailed;
		IUpdatedTriggerEvent     <TEntity, DbContext> ITriggers<TEntity>.Updated      => updated     ;

		IInsertingTriggerEvent   <Object, DbContext> ITriggers.Inserting    => inserting   ;
		IInsertFailedTriggerEvent<Object, DbContext> ITriggers.InsertFailed => insertFailed;
		IInsertedTriggerEvent    <Object, DbContext> ITriggers.Inserted     => inserted    ;
		IDeletingTriggerEvent    <Object, DbContext> ITriggers.Deleting     => deleting    ;
		IDeleteFailedTriggerEvent<Object, DbContext> ITriggers.DeleteFailed => deleteFailed;
		IDeletedTriggerEvent     <Object, DbContext> ITriggers.Deleted      => deleted     ;
		IUpdatingTriggerEvent    <Object, DbContext> ITriggers.Updating     => updating    ;
		IUpdateFailedTriggerEvent<Object, DbContext> ITriggers.UpdateFailed => updateFailed;
		IUpdatedTriggerEvent     <Object, DbContext> ITriggers.Updated      => updated     ;
		
		public static IInsertingTriggerEvent   <TEntity, TDbContext> GlobalInserting    { get; } = new InsertingTriggerEvent   <TEntity, TDbContext>();
		public static IInsertFailedTriggerEvent<TEntity, TDbContext> GlobalInsertFailed { get; } = new InsertFailedTriggerEvent<TEntity, TDbContext>();
		public static IInsertedTriggerEvent    <TEntity, TDbContext> GlobalInserted     { get; } = new InsertedTriggerEvent    <TEntity, TDbContext>();
		public static IDeletingTriggerEvent    <TEntity, TDbContext> GlobalDeleting     { get; } = new DeletingTriggerEvent    <TEntity, TDbContext>();
		public static IDeleteFailedTriggerEvent<TEntity, TDbContext> GlobalDeleteFailed { get; } = new DeleteFailedTriggerEvent<TEntity, TDbContext>();
		public static IDeletedTriggerEvent     <TEntity, TDbContext> GlobalDeleted      { get; } = new DeletedTriggerEvent     <TEntity, TDbContext>();
		public static IUpdatingTriggerEvent    <TEntity, TDbContext> GlobalUpdating     { get; } = new UpdatingTriggerEvent    <TEntity, TDbContext>();
		public static IUpdateFailedTriggerEvent<TEntity, TDbContext> GlobalUpdateFailed { get; } = new UpdateFailedTriggerEvent<TEntity, TDbContext>();
		public static IUpdatedTriggerEvent     <TEntity, TDbContext> GlobalUpdated      { get; } = new UpdatedTriggerEvent     <TEntity, TDbContext>();

		public static event Action<IInsertingEntry   <TEntity, TDbContext>> Inserting    { add => GlobalInserting   .Add(value); remove => GlobalInserting   .Remove(value); }
		public static event Action<IInsertFailedEntry<TEntity, TDbContext>> InsertFailed { add => GlobalInsertFailed.Add(value); remove => GlobalInsertFailed.Remove(value); }
		public static event Action<IInsertedEntry    <TEntity, TDbContext>> Inserted     { add => GlobalInserted    .Add(value); remove => GlobalInserted    .Remove(value); }
		public static event Action<IDeletingEntry    <TEntity, TDbContext>> Deleting     { add => GlobalDeleting    .Add(value); remove => GlobalDeleting    .Remove(value); }
		public static event Action<IDeleteFailedEntry<TEntity, TDbContext>> DeleteFailed { add => GlobalDeleteFailed.Add(value); remove => GlobalDeleteFailed.Remove(value); }
		public static event Action<IDeletedEntry     <TEntity, TDbContext>> Deleted      { add => GlobalDeleted     .Add(value); remove => GlobalDeleted     .Remove(value); }
		public static event Action<IUpdatingEntry    <TEntity, TDbContext>> Updating     { add => GlobalUpdating    .Add(value); remove => GlobalUpdating    .Remove(value); }
		public static event Action<IUpdateFailedEntry<TEntity, TDbContext>> UpdateFailed { add => GlobalUpdateFailed.Add(value); remove => GlobalUpdateFailed.Remove(value); }
		public static event Action<IUpdatedEntry     <TEntity, TDbContext>> Updated      { add => GlobalUpdated     .Add(value); remove => GlobalUpdated     .Remove(value); }

		public override Int32 GetHashCode() => TriggersEqualityComparer<ITriggers<TEntity, TDbContext>>.Instance.GetHashCode(this);
		public override Boolean Equals(Object obj) => obj is ITriggers<TEntity, TDbContext> other && Equals(other);

		public Boolean Equals(ITriggers<TEntity, TDbContext> other) => other is Triggers<TEntity, TDbContext> ted ? ReferenceEquals(this, ted) : TriggersEqualityComparer<ITriggers<TEntity, TDbContext>>.Instance.Equals(this, other);
	}
}
