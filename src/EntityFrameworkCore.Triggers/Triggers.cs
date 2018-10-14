using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	public sealed class Triggers
	: ITriggers
	, IEquatable<ITriggers>
	{
		private readonly ITriggers<Object, DbContext> triggers;
		public Triggers(ITriggers<Object, DbContext> triggers) => this.triggers = triggers;

		IInsertingTriggerEvent   <Object, DbContext> ITriggers.Inserting    => triggers.Inserting   ;
		IInsertFailedTriggerEvent<Object, DbContext> ITriggers.InsertFailed => triggers.InsertFailed;
		IInsertedTriggerEvent    <Object, DbContext> ITriggers.Inserted     => triggers.Inserted    ;
		IDeletingTriggerEvent    <Object, DbContext> ITriggers.Deleting     => triggers.Deleting    ;
		IDeleteFailedTriggerEvent<Object, DbContext> ITriggers.DeleteFailed => triggers.DeleteFailed;
		IDeletedTriggerEvent     <Object, DbContext> ITriggers.Deleted      => triggers.Deleted     ;
		IUpdatingTriggerEvent    <Object, DbContext> ITriggers.Updating     => triggers.Updating    ;
		IUpdateFailedTriggerEvent<Object, DbContext> ITriggers.UpdateFailed => triggers.UpdateFailed;
		IUpdatedTriggerEvent     <Object, DbContext> ITriggers.Updated      => triggers.Updated     ;

		public static IInsertingTriggerEvent   <Object, DbContext> GlobalInserting    { get; } = Triggers<Object>.GlobalInserting   ;
		public static IInsertFailedTriggerEvent<Object, DbContext> GlobalInsertFailed { get; } = Triggers<Object>.GlobalInsertFailed;
		public static IInsertedTriggerEvent    <Object, DbContext> GlobalInserted     { get; } = Triggers<Object>.GlobalInserted    ;
		public static IDeletingTriggerEvent    <Object, DbContext> GlobalDeleting     { get; } = Triggers<Object>.GlobalDeleting    ;
		public static IDeleteFailedTriggerEvent<Object, DbContext> GlobalDeleteFailed { get; } = Triggers<Object>.GlobalDeleteFailed;
		public static IDeletedTriggerEvent     <Object, DbContext> GlobalDeleted      { get; } = Triggers<Object>.GlobalDeleted     ;
		public static IUpdatingTriggerEvent    <Object, DbContext> GlobalUpdating     { get; } = Triggers<Object>.GlobalUpdating    ;
		public static IUpdateFailedTriggerEvent<Object, DbContext> GlobalUpdateFailed { get; } = Triggers<Object>.GlobalUpdateFailed;
		public static IUpdatedTriggerEvent     <Object, DbContext> GlobalUpdated      { get; } = Triggers<Object>.GlobalUpdated     ;

		public static event Action<IInsertingEntry   <Object, DbContext>> Inserting    { add => Triggers<Object>.Inserting    += value; remove => Triggers<Object>.Inserting    -= value; }
		public static event Action<IInsertFailedEntry<Object, DbContext>> InsertFailed { add => Triggers<Object>.InsertFailed += value; remove => Triggers<Object>.InsertFailed -= value; }
		public static event Action<IInsertedEntry    <Object, DbContext>> Inserted     { add => Triggers<Object>.Inserted     += value; remove => Triggers<Object>.Inserted     -= value; }
		public static event Action<IDeletingEntry    <Object, DbContext>> Deleting     { add => Triggers<Object>.Deleting     += value; remove => Triggers<Object>.Deleting     -= value; }
		public static event Action<IDeleteFailedEntry<Object, DbContext>> DeleteFailed { add => Triggers<Object>.DeleteFailed += value; remove => Triggers<Object>.DeleteFailed -= value; }
		public static event Action<IDeletedEntry     <Object, DbContext>> Deleted      { add => Triggers<Object>.Deleted      += value; remove => Triggers<Object>.Deleted      -= value; }
		public static event Action<IUpdatingEntry    <Object, DbContext>> Updating     { add => Triggers<Object>.Updating     += value; remove => Triggers<Object>.Updating     -= value; }
		public static event Action<IUpdateFailedEntry<Object, DbContext>> UpdateFailed { add => Triggers<Object>.UpdateFailed += value; remove => Triggers<Object>.UpdateFailed -= value; }
		public static event Action<IUpdatedEntry     <Object, DbContext>> Updated      { add => Triggers<Object>.Updated      += value; remove => Triggers<Object>.Updated      -= value; }

		public override Int32 GetHashCode() => TriggersEqualityComparer<ITriggers>.Instance.GetHashCode(triggers);
		public override Boolean Equals(Object obj) => obj is ITriggers other && Equals(other);

		public Boolean Equals(ITriggers other) => other is Triggers te ? ReferenceEquals(triggers, te.triggers) : TriggersEqualityComparer<ITriggers>.Instance.Equals(this, other);
	}
}
