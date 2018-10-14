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
		new IInsertingTriggerEvent   <TEntity, TDbContext> Inserting    { get; }
		new IInsertFailedTriggerEvent<TEntity, TDbContext> InsertFailed { get; }
		new IInsertedTriggerEvent    <TEntity, TDbContext> Inserted     { get; }
		new IDeletingTriggerEvent    <TEntity, TDbContext> Deleting     { get; }
		new IDeleteFailedTriggerEvent<TEntity, TDbContext> DeleteFailed { get; }
		new IDeletedTriggerEvent     <TEntity, TDbContext> Deleted      { get; }
		new IUpdatingTriggerEvent    <TEntity, TDbContext> Updating     { get; }
		new IUpdateFailedTriggerEvent<TEntity, TDbContext> UpdateFailed { get; }
		new IUpdatedTriggerEvent     <TEntity, TDbContext> Updated      { get; }
	}

	public interface ITriggers<out TEntity>
	: ITriggers
	where TEntity : class
	{
		new IInsertingTriggerEvent   <TEntity, DbContext> Inserting    { get; }
		new IInsertFailedTriggerEvent<TEntity, DbContext> InsertFailed { get; }
		new IInsertedTriggerEvent    <TEntity, DbContext> Inserted     { get; }
		new IDeletingTriggerEvent    <TEntity, DbContext> Deleting     { get; }
		new IDeleteFailedTriggerEvent<TEntity, DbContext> DeleteFailed { get; }
		new IDeletedTriggerEvent     <TEntity, DbContext> Deleted      { get; }
		new IUpdatingTriggerEvent    <TEntity, DbContext> Updating     { get; }
		new IUpdateFailedTriggerEvent<TEntity, DbContext> UpdateFailed { get; }
		new IUpdatedTriggerEvent     <TEntity, DbContext> Updated      { get; }
	}

	public interface ITriggers
	{
		IInsertingTriggerEvent   <Object, DbContext> Inserting    { get; }
		IInsertFailedTriggerEvent<Object, DbContext> InsertFailed { get; }
		IInsertedTriggerEvent    <Object, DbContext> Inserted     { get; }
		IDeletingTriggerEvent    <Object, DbContext> Deleting     { get; }
		IDeleteFailedTriggerEvent<Object, DbContext> DeleteFailed { get; }
		IDeletedTriggerEvent     <Object, DbContext> Deleted      { get; }
		IUpdatingTriggerEvent    <Object, DbContext> Updating     { get; }
		IUpdateFailedTriggerEvent<Object, DbContext> UpdateFailed { get; }
		IUpdatedTriggerEvent     <Object, DbContext> Updated      { get; }
	}
}