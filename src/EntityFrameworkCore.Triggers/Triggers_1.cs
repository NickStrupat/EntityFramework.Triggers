using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	public sealed class Triggers<TEntity>
	: ITriggers<TEntity>
	, IEquatable<ITriggers<TEntity>>
	where TEntity : class
	{
		private readonly ITriggers<TEntity, DbContext> triggers;
		public Triggers(ITriggers<TEntity, DbContext> triggers) => this.triggers = triggers;

		IInsertingTriggerEvent   <TEntity, DbContext> ITriggers<TEntity>.Inserting    => triggers.Inserting   ;
		IInsertFailedTriggerEvent<TEntity, DbContext> ITriggers<TEntity>.InsertFailed => triggers.InsertFailed;
		IInsertedTriggerEvent    <TEntity, DbContext> ITriggers<TEntity>.Inserted     => triggers.Inserted    ;
		IDeletingTriggerEvent    <TEntity, DbContext> ITriggers<TEntity>.Deleting     => triggers.Deleting    ;
		IDeleteFailedTriggerEvent<TEntity, DbContext> ITriggers<TEntity>.DeleteFailed => triggers.DeleteFailed;
		IDeletedTriggerEvent     <TEntity, DbContext> ITriggers<TEntity>.Deleted      => triggers.Deleted     ;
		IUpdatingTriggerEvent    <TEntity, DbContext> ITriggers<TEntity>.Updating     => triggers.Updating    ;
		IUpdateFailedTriggerEvent<TEntity, DbContext> ITriggers<TEntity>.UpdateFailed => triggers.UpdateFailed;
		IUpdatedTriggerEvent     <TEntity, DbContext> ITriggers<TEntity>.Updated      => triggers.Updated     ;

		IInsertingTriggerEvent   <Object, DbContext> ITriggers.Inserting    => triggers.Inserting   ;
		IInsertFailedTriggerEvent<Object, DbContext> ITriggers.InsertFailed => triggers.InsertFailed;
		IInsertedTriggerEvent    <Object, DbContext> ITriggers.Inserted     => triggers.Inserted    ;
		IDeletingTriggerEvent    <Object, DbContext> ITriggers.Deleting     => triggers.Deleting    ;
		IDeleteFailedTriggerEvent<Object, DbContext> ITriggers.DeleteFailed => triggers.DeleteFailed;
		IDeletedTriggerEvent     <Object, DbContext> ITriggers.Deleted      => triggers.Deleted     ;
		IUpdatingTriggerEvent    <Object, DbContext> ITriggers.Updating     => triggers.Updating    ;
		IUpdateFailedTriggerEvent<Object, DbContext> ITriggers.UpdateFailed => triggers.UpdateFailed;
		IUpdatedTriggerEvent     <Object, DbContext> ITriggers.Updated      => triggers.Updated     ;

		public static IInsertingTriggerEvent   <TEntity, DbContext> GlobalInserting    { get; } = Triggers<TEntity, DbContext>.GlobalInserting   ;
		public static IInsertFailedTriggerEvent<TEntity, DbContext> GlobalInsertFailed { get; } = Triggers<TEntity, DbContext>.GlobalInsertFailed;
		public static IInsertedTriggerEvent    <TEntity, DbContext> GlobalInserted     { get; } = Triggers<TEntity, DbContext>.GlobalInserted    ;
		public static IDeletingTriggerEvent    <TEntity, DbContext> GlobalDeleting     { get; } = Triggers<TEntity, DbContext>.GlobalDeleting    ;
		public static IDeleteFailedTriggerEvent<TEntity, DbContext> GlobalDeleteFailed { get; } = Triggers<TEntity, DbContext>.GlobalDeleteFailed;
		public static IDeletedTriggerEvent     <TEntity, DbContext> GlobalDeleted      { get; } = Triggers<TEntity, DbContext>.GlobalDeleted     ;
		public static IUpdatingTriggerEvent    <TEntity, DbContext> GlobalUpdating     { get; } = Triggers<TEntity, DbContext>.GlobalUpdating    ;
		public static IUpdateFailedTriggerEvent<TEntity, DbContext> GlobalUpdateFailed { get; } = Triggers<TEntity, DbContext>.GlobalUpdateFailed;
		public static IUpdatedTriggerEvent     <TEntity, DbContext> GlobalUpdated      { get; } = Triggers<TEntity, DbContext>.GlobalUpdated     ;

		public static event Action<IInsertingEntry   <TEntity, DbContext>> Inserting    { add => Triggers<TEntity, DbContext>.Inserting    += value; remove => Triggers<TEntity, DbContext>.Inserting    -= value; }
		public static event Action<IInsertFailedEntry<TEntity, DbContext>> InsertFailed { add => Triggers<TEntity, DbContext>.InsertFailed += value; remove => Triggers<TEntity, DbContext>.InsertFailed -= value; }
		public static event Action<IInsertedEntry    <TEntity, DbContext>> Inserted     { add => Triggers<TEntity, DbContext>.Inserted     += value; remove => Triggers<TEntity, DbContext>.Inserted     -= value; }
		public static event Action<IDeletingEntry    <TEntity, DbContext>> Deleting     { add => Triggers<TEntity, DbContext>.Deleting     += value; remove => Triggers<TEntity, DbContext>.Deleting     -= value; }
		public static event Action<IDeleteFailedEntry<TEntity, DbContext>> DeleteFailed { add => Triggers<TEntity, DbContext>.DeleteFailed += value; remove => Triggers<TEntity, DbContext>.DeleteFailed -= value; }
		public static event Action<IDeletedEntry     <TEntity, DbContext>> Deleted      { add => Triggers<TEntity, DbContext>.Deleted      += value; remove => Triggers<TEntity, DbContext>.Deleted      -= value; }
		public static event Action<IUpdatingEntry    <TEntity, DbContext>> Updating     { add => Triggers<TEntity, DbContext>.Updating     += value; remove => Triggers<TEntity, DbContext>.Updating     -= value; }
		public static event Action<IUpdateFailedEntry<TEntity, DbContext>> UpdateFailed { add => Triggers<TEntity, DbContext>.UpdateFailed += value; remove => Triggers<TEntity, DbContext>.UpdateFailed -= value; }
		public static event Action<IUpdatedEntry     <TEntity, DbContext>> Updated      { add => Triggers<TEntity, DbContext>.Updated      += value; remove => Triggers<TEntity, DbContext>.Updated      -= value; }

		public override Int32 GetHashCode() => TriggersEqualityComparer<ITriggers<TEntity>>.Instance.GetHashCode(triggers);
		public override Boolean Equals(Object obj) => obj is ITriggers<TEntity, DbContext> other && Equals(other);

		public Boolean Equals(ITriggers<TEntity> other) => other is Triggers<TEntity> te ? ReferenceEquals(triggers, te.triggers) : TriggersEqualityComparer<ITriggers<TEntity>>.Instance.Equals(this, other);
	}
}