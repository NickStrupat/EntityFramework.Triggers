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

		ITriggerEvent<IInsertingEntry   <TEntity, DbContext>, TEntity> ITriggers<TEntity>.Inserting    => triggers.Inserting   ;
		ITriggerEvent<IInsertFailedEntry<TEntity, DbContext>, TEntity> ITriggers<TEntity>.InsertFailed => triggers.InsertFailed;
		ITriggerEvent<IInsertedEntry    <TEntity, DbContext>, TEntity> ITriggers<TEntity>.Inserted     => triggers.Inserted    ;
		ITriggerEvent<IDeletingEntry    <TEntity, DbContext>, TEntity> ITriggers<TEntity>.Deleting     => triggers.Deleting    ;
		ITriggerEvent<IDeleteFailedEntry<TEntity, DbContext>, TEntity> ITriggers<TEntity>.DeleteFailed => triggers.DeleteFailed;
		ITriggerEvent<IDeletedEntry     <TEntity, DbContext>, TEntity> ITriggers<TEntity>.Deleted      => triggers.Deleted     ;
		ITriggerEvent<IUpdatingEntry    <TEntity, DbContext>, TEntity> ITriggers<TEntity>.Updating     => triggers.Updating    ;
		ITriggerEvent<IUpdateFailedEntry<TEntity, DbContext>, TEntity> ITriggers<TEntity>.UpdateFailed => triggers.UpdateFailed;
		ITriggerEvent<IUpdatedEntry     <TEntity, DbContext>, TEntity> ITriggers<TEntity>.Updated      => triggers.Updated     ;

		ITriggerEvent<IInsertingEntry   <Object, DbContext>> ITriggers.Inserting    => triggers.Inserting   ;
		ITriggerEvent<IInsertFailedEntry<Object, DbContext>> ITriggers.InsertFailed => triggers.InsertFailed;
		ITriggerEvent<IInsertedEntry    <Object, DbContext>> ITriggers.Inserted     => triggers.Inserted    ;
		ITriggerEvent<IDeletingEntry    <Object, DbContext>> ITriggers.Deleting     => triggers.Deleting    ;
		ITriggerEvent<IDeleteFailedEntry<Object, DbContext>> ITriggers.DeleteFailed => triggers.DeleteFailed;
		ITriggerEvent<IDeletedEntry     <Object, DbContext>> ITriggers.Deleted      => triggers.Deleted     ;
		ITriggerEvent<IUpdatingEntry    <Object, DbContext>> ITriggers.Updating     => triggers.Updating    ;
		ITriggerEvent<IUpdateFailedEntry<Object, DbContext>> ITriggers.UpdateFailed => triggers.UpdateFailed;
		ITriggerEvent<IUpdatedEntry     <Object, DbContext>> ITriggers.Updated      => triggers.Updated     ;

		public static ITriggerEvent<IInsertingEntry   <TEntity, DbContext>, TEntity, DbContext> GlobalInserting    { get; } = Triggers<TEntity, DbContext>.GlobalInserting   ;
		public static ITriggerEvent<IInsertFailedEntry<TEntity, DbContext>, TEntity, DbContext> GlobalInsertFailed { get; } = Triggers<TEntity, DbContext>.GlobalInsertFailed;
		public static ITriggerEvent<IInsertedEntry    <TEntity, DbContext>, TEntity, DbContext> GlobalInserted     { get; } = Triggers<TEntity, DbContext>.GlobalInserted    ;
		public static ITriggerEvent<IDeletingEntry    <TEntity, DbContext>, TEntity, DbContext> GlobalDeleting     { get; } = Triggers<TEntity, DbContext>.GlobalDeleting    ;
		public static ITriggerEvent<IDeleteFailedEntry<TEntity, DbContext>, TEntity, DbContext> GlobalDeleteFailed { get; } = Triggers<TEntity, DbContext>.GlobalDeleteFailed;
		public static ITriggerEvent<IDeletedEntry     <TEntity, DbContext>, TEntity, DbContext> GlobalDeleted      { get; } = Triggers<TEntity, DbContext>.GlobalDeleted     ;
		public static ITriggerEvent<IUpdatingEntry    <TEntity, DbContext>, TEntity, DbContext> GlobalUpdating     { get; } = Triggers<TEntity, DbContext>.GlobalUpdating    ;
		public static ITriggerEvent<IUpdateFailedEntry<TEntity, DbContext>, TEntity, DbContext> GlobalUpdateFailed { get; } = Triggers<TEntity, DbContext>.GlobalUpdateFailed;
		public static ITriggerEvent<IUpdatedEntry     <TEntity, DbContext>, TEntity, DbContext> GlobalUpdated      { get; } = Triggers<TEntity, DbContext>.GlobalUpdated     ;

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