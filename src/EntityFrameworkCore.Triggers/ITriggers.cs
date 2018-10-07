#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	public interface ITriggers<TEntity, TDbContext>
	: ITriggers
	where TEntity : class
	where TDbContext : DbContext
	{
		new TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext> Deleted      { get; }
		new TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> DeleteFailed { get; }
		new TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext> Deleting     { get; }
		new TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext> Inserted     { get; }
		new TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> InsertFailed { get; }
		new TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext> Inserting    { get; }
		new TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext> Updated      { get; }
		new TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> UpdateFailed { get; }
		new TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext> Updating     { get; }
	}

	public interface ITriggers<TEntity> : ITriggers<TEntity, DbContext>
	where TEntity : class {}

	public interface ITriggers
	{
		TriggerEvent Deleted      { get; }
		TriggerEvent DeleteFailed { get; }
		TriggerEvent Deleting     { get; }
		TriggerEvent Inserted     { get; }
		TriggerEvent InsertFailed { get; }
		TriggerEvent Inserting    { get; }
		TriggerEvent Updated      { get; }
		TriggerEvent UpdateFailed { get; }
		TriggerEvent Updating     { get; }
	}
}