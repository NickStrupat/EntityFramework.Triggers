using System;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	public interface ITriggers<out TEntity, out TDbContext>
	: ITriggers<TEntity>
	where TEntity : class
	where TDbContext : DbContext
	{
		new ITriggerEvent<IInsertingEntry   <TEntity, TDbContext>, TEntity, TDbContext> Inserting    { get; }
		new ITriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> InsertFailed { get; }
		new ITriggerEvent<IInsertedEntry    <TEntity, TDbContext>, TEntity, TDbContext> Inserted     { get; }
		new ITriggerEvent<IDeletingEntry    <TEntity, TDbContext>, TEntity, TDbContext> Deleting     { get; }
		new ITriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> DeleteFailed { get; }
		new ITriggerEvent<IDeletedEntry     <TEntity, TDbContext>, TEntity, TDbContext> Deleted      { get; }
		new ITriggerEvent<IUpdatingEntry    <TEntity, TDbContext>, TEntity, TDbContext> Updating     { get; }
		new ITriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext> UpdateFailed { get; }
		new ITriggerEvent<IUpdatedEntry     <TEntity, TDbContext>, TEntity, TDbContext> Updated      { get; }
	}

	public interface ITriggers<out TEntity>
	: ITriggers
	where TEntity : class
	{
		new ITriggerEvent<IInsertingEntry   <TEntity, DbContext>, TEntity> Inserting    { get; }
		new ITriggerEvent<IInsertFailedEntry<TEntity, DbContext>, TEntity> InsertFailed { get; }
		new ITriggerEvent<IInsertedEntry    <TEntity, DbContext>, TEntity> Inserted     { get; }
		new ITriggerEvent<IDeletingEntry    <TEntity, DbContext>, TEntity> Deleting     { get; }
		new ITriggerEvent<IDeleteFailedEntry<TEntity, DbContext>, TEntity> DeleteFailed { get; }
		new ITriggerEvent<IDeletedEntry     <TEntity, DbContext>, TEntity> Deleted      { get; }
		new ITriggerEvent<IUpdatingEntry    <TEntity, DbContext>, TEntity> Updating     { get; }
		new ITriggerEvent<IUpdateFailedEntry<TEntity, DbContext>, TEntity> UpdateFailed { get; }
		new ITriggerEvent<IUpdatedEntry     <TEntity, DbContext>, TEntity> Updated      { get; }
	}

	public interface ITriggers
	{
		ITriggerEvent<IInsertingEntry   <Object, DbContext>> Inserting    { get; }
		ITriggerEvent<IInsertFailedEntry<Object, DbContext>> InsertFailed { get; }
		ITriggerEvent<IInsertedEntry    <Object, DbContext>> Inserted     { get; }
		ITriggerEvent<IDeletingEntry    <Object, DbContext>> Deleting     { get; }
		ITriggerEvent<IDeleteFailedEntry<Object, DbContext>> DeleteFailed { get; }
		ITriggerEvent<IDeletedEntry     <Object, DbContext>> Deleted      { get; }
		ITriggerEvent<IUpdatingEntry    <Object, DbContext>> Updating     { get; }
		ITriggerEvent<IUpdateFailedEntry<Object, DbContext>> UpdateFailed { get; }
		ITriggerEvent<IUpdatedEntry     <Object, DbContext>> Updated      { get; }
	}
}