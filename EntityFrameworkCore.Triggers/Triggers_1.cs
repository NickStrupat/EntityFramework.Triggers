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
	, IEquatable<ITriggers<TEntity, DbContext>>
	where TEntity : class
	{
		private readonly ITriggers<TEntity, DbContext> triggers;
		public Triggers(ITriggers<TEntity, DbContext> triggers) => this.triggers = triggers;

		TriggerEvent<IInsertingEntry   <TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.Inserting    => triggers.Inserting   ;
		TriggerEvent<IInsertFailedEntry<TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.InsertFailed => triggers.InsertFailed;
		TriggerEvent<IInsertedEntry    <TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.Inserted     => triggers.Inserted    ;
		TriggerEvent<IDeletingEntry    <TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.Deleting     => triggers.Deleting    ;
		TriggerEvent<IDeleteFailedEntry<TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.DeleteFailed => triggers.DeleteFailed;
		TriggerEvent<IDeletedEntry     <TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.Deleted      => triggers.Deleted     ;
		TriggerEvent<IUpdatingEntry    <TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.Updating     => triggers.Updating    ;
		TriggerEvent<IUpdateFailedEntry<TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.UpdateFailed => triggers.UpdateFailed;
		TriggerEvent<IUpdatedEntry     <TEntity, DbContext>, TEntity, DbContext> ITriggers<TEntity, DbContext>.Updated      => triggers.Updated     ;

		TriggerEvent ITriggers.Inserting    => ((ITriggers<TEntity, DbContext>) this).Inserting   ;
		TriggerEvent ITriggers.InsertFailed => ((ITriggers<TEntity, DbContext>) this).InsertFailed;
		TriggerEvent ITriggers.Inserted     => ((ITriggers<TEntity, DbContext>) this).Inserted    ;
		TriggerEvent ITriggers.Deleting     => ((ITriggers<TEntity, DbContext>) this).Deleting    ;
		TriggerEvent ITriggers.DeleteFailed => ((ITriggers<TEntity, DbContext>) this).DeleteFailed;
		TriggerEvent ITriggers.Deleted      => ((ITriggers<TEntity, DbContext>) this).Deleted     ;
		TriggerEvent ITriggers.Updating     => ((ITriggers<TEntity, DbContext>) this).Updating    ;
		TriggerEvent ITriggers.UpdateFailed => ((ITriggers<TEntity, DbContext>) this).UpdateFailed;
		TriggerEvent ITriggers.Updated      => ((ITriggers<TEntity, DbContext>) this).Updated     ;

		public static TriggerEvent<IInsertingEntry   <TEntity, DbContext>, TEntity, DbContext> GlobalInserting    { get; } = Triggers<TEntity, DbContext>.GlobalInserting   ;
		public static TriggerEvent<IInsertFailedEntry<TEntity, DbContext>, TEntity, DbContext> GlobalInsertFailed { get; } = Triggers<TEntity, DbContext>.GlobalInsertFailed;
		public static TriggerEvent<IInsertedEntry    <TEntity, DbContext>, TEntity, DbContext> GlobalInserted     { get; } = Triggers<TEntity, DbContext>.GlobalInserted    ;
		public static TriggerEvent<IDeletingEntry    <TEntity, DbContext>, TEntity, DbContext> GlobalDeleting     { get; } = Triggers<TEntity, DbContext>.GlobalDeleting    ;
		public static TriggerEvent<IDeleteFailedEntry<TEntity, DbContext>, TEntity, DbContext> GlobalDeleteFailed { get; } = Triggers<TEntity, DbContext>.GlobalDeleteFailed;
		public static TriggerEvent<IDeletedEntry     <TEntity, DbContext>, TEntity, DbContext> GlobalDeleted      { get; } = Triggers<TEntity, DbContext>.GlobalDeleted     ;
		public static TriggerEvent<IUpdatingEntry    <TEntity, DbContext>, TEntity, DbContext> GlobalUpdating     { get; } = Triggers<TEntity, DbContext>.GlobalUpdating    ;
		public static TriggerEvent<IUpdateFailedEntry<TEntity, DbContext>, TEntity, DbContext> GlobalUpdateFailed { get; } = Triggers<TEntity, DbContext>.GlobalUpdateFailed;
		public static TriggerEvent<IUpdatedEntry     <TEntity, DbContext>, TEntity, DbContext> GlobalUpdated      { get; } = Triggers<TEntity, DbContext>.GlobalUpdated     ;

		public static event Action<IInsertingEntry   <TEntity, DbContext>> Inserting    { add => Triggers<TEntity, DbContext>.Inserting    += value; remove => Triggers<TEntity, DbContext>.Inserting    -= value; }
		public static event Action<IInsertFailedEntry<TEntity, DbContext>> InsertFailed { add => Triggers<TEntity, DbContext>.InsertFailed += value; remove => Triggers<TEntity, DbContext>.InsertFailed -= value; }
		public static event Action<IInsertedEntry    <TEntity, DbContext>> Inserted     { add => Triggers<TEntity, DbContext>.Inserted     += value; remove => Triggers<TEntity, DbContext>.Inserted     -= value; }
		public static event Action<IDeletingEntry    <TEntity, DbContext>> Deleting     { add => Triggers<TEntity, DbContext>.Deleting     += value; remove => Triggers<TEntity, DbContext>.Deleting     -= value; }
		public static event Action<IDeleteFailedEntry<TEntity, DbContext>> DeleteFailed { add => Triggers<TEntity, DbContext>.DeleteFailed += value; remove => Triggers<TEntity, DbContext>.DeleteFailed -= value; }
		public static event Action<IDeletedEntry     <TEntity, DbContext>> Deleted      { add => Triggers<TEntity, DbContext>.Deleted      += value; remove => Triggers<TEntity, DbContext>.Deleted      -= value; }
		public static event Action<IUpdatingEntry    <TEntity, DbContext>> Updating     { add => Triggers<TEntity, DbContext>.Updating     += value; remove => Triggers<TEntity, DbContext>.Updating     -= value; }
		public static event Action<IUpdateFailedEntry<TEntity, DbContext>> UpdateFailed { add => Triggers<TEntity, DbContext>.UpdateFailed += value; remove => Triggers<TEntity, DbContext>.UpdateFailed -= value; }
		public static event Action<IUpdatedEntry     <TEntity, DbContext>> Updated      { add => Triggers<TEntity, DbContext>.Updated      += value; remove => Triggers<TEntity, DbContext>.Updated      -= value; }

		public override Int32 GetHashCode() => TriggersEqualityComparer<TEntity, DbContext>.Instance.GetHashCode(triggers);
		public override Boolean Equals(Object obj) => obj is ITriggers<TEntity, DbContext> other && Equals(other);

		public Boolean Equals(ITriggers<TEntity, DbContext> other) => other is Triggers<TEntity> te ? ReferenceEquals(triggers, te.triggers) : TriggersEqualityComparer<TEntity, DbContext>.Instance.Equals(this, other);
	}
}