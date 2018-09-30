#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	public interface ITriggers<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		TriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext> Deleted      { get; }
		TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> DeleteFailed { get; }
		TriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext> Deleting     { get; }
		TriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext> Inserted     { get; }
		TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> InsertFailed { get; }
		TriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext> Inserting    { get; }
		TriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext> Updated      { get; }
		TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> UpdateFailed { get; }
		TriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext> Updating     { get; }
	}

    public interface ITriggers<TEntity> : ITriggers<TEntity, DbContext>
    where TEntity : class {}
}