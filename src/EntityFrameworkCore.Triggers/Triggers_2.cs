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
		TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Inserting    { get; } = new TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext>();
		TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.InsertFailed { get; } = new TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>();
		TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Inserted     { get; } = new TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext>();
		TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Deleting     { get; } = new TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext>();
		TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.DeleteFailed { get; } = new TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>();
		TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Deleted      { get; } = new TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext>();
		TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Updating     { get; } = new TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext>();
		TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.UpdateFailed { get; } = new TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>();
		TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext> ITriggers<TEntity, TDbContext>.Updated      { get; } = new TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext>();

		TriggerEvent ITriggers.Inserting    => ((ITriggers<TEntity, TDbContext>) this).Inserting   ;
		TriggerEvent ITriggers.InsertFailed => ((ITriggers<TEntity, TDbContext>) this).InsertFailed;
		TriggerEvent ITriggers.Inserted     => ((ITriggers<TEntity, TDbContext>) this).Inserted    ;
		TriggerEvent ITriggers.Deleting     => ((ITriggers<TEntity, TDbContext>) this).Deleting    ;
		TriggerEvent ITriggers.DeleteFailed => ((ITriggers<TEntity, TDbContext>) this).DeleteFailed;
		TriggerEvent ITriggers.Deleted      => ((ITriggers<TEntity, TDbContext>) this).Deleted     ;
		TriggerEvent ITriggers.Updating     => ((ITriggers<TEntity, TDbContext>) this).Updating    ;
		TriggerEvent ITriggers.UpdateFailed => ((ITriggers<TEntity, TDbContext>) this).UpdateFailed;
		TriggerEvent ITriggers.Updated      => ((ITriggers<TEntity, TDbContext>) this).Updated     ;
		
		public static TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext> GlobalInserting    { get; } = new TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext>();
		public static TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> GlobalInsertFailed { get; } = new TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>();
		public static TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext> GlobalInserted     { get; } = new TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext>();
		public static TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext> GlobalDeleting     { get; } = new TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext>();
		public static TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> GlobalDeleteFailed { get; } = new TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>();
		public static TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext> GlobalDeleted      { get; } = new TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext>();
		public static TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext> GlobalUpdating     { get; } = new TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext>();
		public static TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> GlobalUpdateFailed { get; } = new TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>();
		public static TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext> GlobalUpdated      { get; } = new TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext>();

		public static event Action<IInsertingEntry   <TEntity, TDbContext>> Inserting    { add => GlobalInserting   .Add(value); remove => GlobalInserting   .Remove(value); }
		public static event Action<IInsertFailedEntry<TEntity, TDbContext>> InsertFailed { add => GlobalInsertFailed.Add(value); remove => GlobalInsertFailed.Remove(value); }
		public static event Action<IInsertedEntry    <TEntity, TDbContext>> Inserted     { add => GlobalInserted    .Add(value); remove => GlobalInserted    .Remove(value); }
		public static event Action<IDeletingEntry    <TEntity, TDbContext>> Deleting     { add => GlobalDeleting    .Add(value); remove => GlobalDeleting    .Remove(value); }
		public static event Action<IDeleteFailedEntry<TEntity, TDbContext>> DeleteFailed { add => GlobalDeleteFailed.Add(value); remove => GlobalDeleteFailed.Remove(value); }
		public static event Action<IDeletedEntry     <TEntity, TDbContext>> Deleted      { add => GlobalDeleted     .Add(value); remove => GlobalDeleted     .Remove(value); }
		public static event Action<IUpdatingEntry    <TEntity, TDbContext>> Updating     { add => GlobalUpdating    .Add(value); remove => GlobalUpdating    .Remove(value); }
		public static event Action<IUpdateFailedEntry<TEntity, TDbContext>> UpdateFailed { add => GlobalUpdateFailed.Add(value); remove => GlobalUpdateFailed.Remove(value); }
		public static event Action<IUpdatedEntry     <TEntity, TDbContext>> Updated      { add => GlobalUpdated     .Add(value); remove => GlobalUpdated     .Remove(value); }

		public override Int32 GetHashCode() => TriggersEqualityComparer<TEntity, TDbContext>.Instance.GetHashCode(this);
		public override Boolean Equals(Object obj) => obj is ITriggers<TEntity, TDbContext> other && Equals(other);

		public Boolean Equals(ITriggers<TEntity, TDbContext> other) => other is Triggers<TEntity, TDbContext> ted ? ReferenceEquals(this, ted) : TriggersEqualityComparer<TEntity, TDbContext>.Instance.Equals(this, other);

		public class InsertingTriggerEvent : TriggerEvent<IInsertingEntry<TEntity, TDbContext>, TEntity, TDbContext>
		{
			public void Add<TService>(Action<IInsertingEntry<TEntity, TDbContext, TService>> handler)
			{
				void Wrapper(IInsertingEntry<TEntity, TDbContext> entry, IServiceProvider sp)
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
}
