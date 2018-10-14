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
		private readonly ITriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext> inserting    = new TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext>();
		private readonly ITriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> insertFailed = new TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>();
		private readonly ITriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext> inserted     = new TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext>();
		private readonly ITriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext> deleting     = new TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext>();
		private readonly ITriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> deleteFailed = new TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>();
		private readonly ITriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext> deleted      = new TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext>();
		private readonly ITriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext> updating     = new TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext>();
		private readonly ITriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> updateFailed = new TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>();
		private readonly ITriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext> updated      = new TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext>();

		ITriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Inserting    => inserting   ;
		ITriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.InsertFailed => insertFailed;
		ITriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Inserted     => inserted    ;
		ITriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Deleting     => deleting    ;
		ITriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.DeleteFailed => deleteFailed;
		ITriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Deleted      => deleted     ;
		ITriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Updating     => updating    ;
		ITriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.UpdateFailed => updateFailed;
		ITriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Updated      => updated     ;

		ITriggerEvent<IInsertingEntry   <TEntity, DbContext>, TEntity> ITriggers<TEntity>.Inserting    => inserting   ;
		ITriggerEvent<IInsertFailedEntry<TEntity, DbContext>, TEntity> ITriggers<TEntity>.InsertFailed => insertFailed;
		ITriggerEvent<IInsertedEntry    <TEntity, DbContext>, TEntity> ITriggers<TEntity>.Inserted     => inserted    ;
		ITriggerEvent<IDeletingEntry    <TEntity, DbContext>, TEntity> ITriggers<TEntity>.Deleting     => deleting    ;
		ITriggerEvent<IDeleteFailedEntry<TEntity, DbContext>, TEntity> ITriggers<TEntity>.DeleteFailed => deleteFailed;
		ITriggerEvent<IDeletedEntry     <TEntity, DbContext>, TEntity> ITriggers<TEntity>.Deleted      => deleted     ;
		ITriggerEvent<IUpdatingEntry    <TEntity, DbContext>, TEntity> ITriggers<TEntity>.Updating     => updating    ;
		ITriggerEvent<IUpdateFailedEntry<TEntity, DbContext>, TEntity> ITriggers<TEntity>.UpdateFailed => updateFailed;
		ITriggerEvent<IUpdatedEntry     <TEntity, DbContext>, TEntity> ITriggers<TEntity>.Updated      => updated     ;

		ITriggerEvent<IInsertingEntry   <Object, DbContext>> ITriggers.Inserting    => inserting   ;
		ITriggerEvent<IInsertFailedEntry<Object, DbContext>> ITriggers.InsertFailed => insertFailed;
		ITriggerEvent<IInsertedEntry    <Object, DbContext>> ITriggers.Inserted     => inserted    ;
		ITriggerEvent<IDeletingEntry    <Object, DbContext>> ITriggers.Deleting     => deleting    ;
		ITriggerEvent<IDeleteFailedEntry<Object, DbContext>> ITriggers.DeleteFailed => deleteFailed;
		ITriggerEvent<IDeletedEntry     <Object, DbContext>> ITriggers.Deleted      => deleted     ;
		ITriggerEvent<IUpdatingEntry    <Object, DbContext>> ITriggers.Updating     => updating    ;
		ITriggerEvent<IUpdateFailedEntry<Object, DbContext>> ITriggers.UpdateFailed => updateFailed;
		ITriggerEvent<IUpdatedEntry     <Object, DbContext>> ITriggers.Updated      => updated     ;
		
		public static ITriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext> GlobalInserting    { get; } = new TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext>();
		public static ITriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> GlobalInsertFailed { get; } = new TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>();
		public static ITriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext> GlobalInserted     { get; } = new TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext>();
		public static ITriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext> GlobalDeleting     { get; } = new TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext>();
		public static ITriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> GlobalDeleteFailed { get; } = new TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>();
		public static ITriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext> GlobalDeleted      { get; } = new TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext>();
		public static ITriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext> GlobalUpdating     { get; } = new TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext>();
		public static ITriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> GlobalUpdateFailed { get; } = new TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>();
		public static ITriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext> GlobalUpdated      { get; } = new TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext>();

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

	public class InsertingTriggerEvent<TEntity, TDbContext> : TriggerEvent<IInsertingEntry<TEntity, TDbContext>, TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public void Add<TService>(Action<IInsertingEntry<TEntity, TDbContext, TService>> handler)
		{
			void Wrapper(IInsertingEntry<TEntity, TDbContext> entry)
			{
				var insertingEntry = new InsertingEntry<TEntity, TDbContext, TService>(entry.Entity, entry.Context, entry.Service, entry.Cancel);
				handler.Invoke(insertingEntry);
				entry.Cancel = insertingEntry.Cancel;
			}
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TService>(Action<IInsertingEntry<TEntity, TDbContext, TService>> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}
	}
}
