using System;

using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Triggers;

#region Base classes

#region Non-generic service

internal abstract class Entry<TEntity, TDbContext>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service)
	: IEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
{
	public TEntity Entity { get; } = entity;
	Object IEntry.Entity => Entity;
	public TDbContext Context { get; } = context;
	public IServiceProvider Service { get; } = service;
	DbContext IEntry.Context => Context;
}

internal abstract class BeforeEntry<TEntity, TDbContext>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Boolean cancel)
	: Entry<TEntity, TDbContext>(entity, context, service), IBeforeEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
{
	public Boolean Cancel { get; set; } = cancel;
}

internal abstract class BeforeChangeEntry<TEntity, TDbContext>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Boolean cancel)
	: BeforeEntry<TEntity, TDbContext>(entity, context, service, cancel), IBeforeChangeEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
{
	private TEntity? original;
	public TEntity Original => original ??= (TEntity)Context.Entry(Entity).OriginalValues.ToObject();
}

internal abstract class FailedEntry<TEntity, TDbContext>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Exception exception,
	Boolean swallow)
	: Entry<TEntity, TDbContext>(entity, context, service), IFailedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
{
	public Exception Exception { get; } = exception;
	public Boolean Swallow { get; set; } = swallow;
}

internal abstract class ChangeFailedEntry<TEntity, TDbContext>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Exception exception,
	Boolean swallow)
	: FailedEntry<TEntity, TDbContext>(entity, context, service, exception, swallow)
		, IChangeFailedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext;

#endregion

#region Generic service

internal abstract class Entry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service)
	: Entry<TEntity, TDbContext>(entity, context, service), IEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
{
	public new TService Service { get; } = ServiceRetrieval<TService>.GetService(service);
}

internal abstract class BeforeEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Boolean cancel)
	: Entry<TEntity, TDbContext, TService>(entity, context, service), IBeforeEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
{
	public Boolean Cancel { get; set; } = cancel;
}

internal abstract class BeforeChangeEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Boolean cancel)
	: BeforeEntry<TEntity, TDbContext, TService>(entity, context, service, cancel),
		IBeforeChangeEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
{
	private TEntity? original;
	public TEntity Original => original ??= (TEntity)Context.Entry(Entity).OriginalValues.ToObject();
}

internal abstract class FailedEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Exception exception,
	Boolean swallow)
	: Entry<TEntity, TDbContext, TService>(entity, context, service), IFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext
{
	public Exception Exception { get; } = exception;
	public Boolean Swallow { get; set; } = swallow;
}

internal abstract class ChangeFailedEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Exception exception,
	Boolean swallow)
	: FailedEntry<TEntity, TDbContext, TService>(entity, context, service, exception, swallow),
		IChangeFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

#endregion

#region Wrapped with generic service

internal abstract class WrappedEntry<TEntry, TEntity, TDbContext, TService>(TEntry entry)
	: IEntry<TEntity, TDbContext, TService>
	where TEntry : class, IEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
{
	protected readonly TEntry Entry = entry;

	Object IEntry.Entity => Entity;
	public TDbContext Context => Entry.Context;
	public TEntity Entity => Entry.Entity;
	DbContext IEntry.Context => Context;
	IServiceProvider IEntry.Service => Entry.Service;
	public TService Service { get; } = ServiceRetrieval<TService>.GetService(entry.Service);
}

internal abstract class WrappedBeforeEntry<TEntry, TEntity, TDbContext, TService>(TEntry entry)
	: WrappedEntry<TEntry, TEntity, TDbContext, TService>(entry), IBeforeEntry<TEntity, TDbContext, TService>
	where TEntry : class, IBeforeEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
{
	public Boolean Cancel
	{
		get => Entry.Cancel;
		set => Entry.Cancel = value;
	}
}

internal abstract class WrappedBeforeChangeEntry<TEntry, TEntity, TDbContext, TService>(TEntry entry)
	: WrappedBeforeEntry<TEntry, TEntity, TDbContext, TService>(entry),
		IBeforeChangeEntry<TEntity, TDbContext, TService>
	where TEntry : class, IBeforeChangeEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
{
	public TEntity Original => Entry.Original;
}

internal abstract class WrappedFailedEntry<TEntry, TEntity, TDbContext, TService>(TEntry entry)
	: WrappedEntry<TEntry, TEntity, TDbContext, TService>(entry), IFailedEntry<TEntity, TDbContext, TService>
	where TEntry : class, IFailedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
{
	public Exception Exception => Entry.Exception;

	public Boolean Swallow
	{
		get => Entry.Swallow;
		set => Entry.Swallow = value;
	}
}

internal abstract class WrappedChangeFailedEntry<TEntry, TEntity, TDbContext, TService>(TEntry entry)
	: WrappedFailedEntry<TEntry, TEntity, TDbContext, TService>(entry),
		IChangeFailedEntry<TEntity, TDbContext, TService>
	where TEntry : class, IChangeFailedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext;

#endregion

#endregion

#region Final classes

#region Non-generic service

internal class InsertingEntry<TEntity, TDbContext>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Boolean cancel)
	: BeforeEntry<TEntity, TDbContext>(entity, context, service, cancel), IInsertingEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext;

internal class UpdatingEntry<TEntity, TDbContext>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Boolean cancel)
	: BeforeChangeEntry<TEntity, TDbContext>(entity, context, service, cancel), IUpdatingEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext;

internal class DeletingEntry<TEntity, TDbContext>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Boolean cancel)
	: BeforeChangeEntry<TEntity, TDbContext>(entity, context, service, cancel), IDeletingEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext;

internal class InsertFailedEntry<TEntity, TDbContext>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Exception exception,
	Boolean swallow)
	: FailedEntry<TEntity, TDbContext>(entity, context, service, exception, swallow),
		IInsertFailedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext;

internal class UpdateFailedEntry<TEntity, TDbContext>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Exception exception,
	Boolean swallow)
	: ChangeFailedEntry<TEntity, TDbContext>(entity, context, service, exception, swallow),
		IUpdateFailedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext;

internal class DeleteFailedEntry<TEntity, TDbContext>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Exception exception,
	Boolean swallow)
	: ChangeFailedEntry<TEntity, TDbContext>(entity, context, service, exception, swallow),
		IDeleteFailedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext;

internal class InsertedEntry<TEntity, TDbContext>(TEntity entity, TDbContext context, IServiceProvider service)
	: Entry<TEntity, TDbContext>(entity, context,
		service), IInsertedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext;

internal class UpdatedEntry<TEntity, TDbContext>(TEntity entity, TDbContext context, IServiceProvider service)
	: Entry<TEntity, TDbContext>(entity, context,
		service), IUpdatedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext;

internal class DeletedEntry<TEntity, TDbContext>(TEntity entity, TDbContext context, IServiceProvider service)
	: Entry<TEntity, TDbContext>(entity, context,
		service), IDeletedEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext;

#endregion

#region Generic service

internal class InsertingEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Boolean cancel)
	: BeforeEntry<TEntity, TDbContext, TService>(entity, context, service, cancel),
		IInsertingEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class UpdatingEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Boolean cancel)
	: BeforeChangeEntry<TEntity, TDbContext, TService>(entity, context, service, cancel),
		IUpdatingEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class DeletingEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Boolean cancel)
	: BeforeChangeEntry<TEntity, TDbContext, TService>(entity, context, service, cancel),
		IDeletingEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class InsertFailedEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Exception exception,
	Boolean swallow)
	: FailedEntry<TEntity, TDbContext, TService>(entity, context, service, exception, swallow),
		IInsertFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class UpdateFailedEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Exception exception,
	Boolean swallow)
	: ChangeFailedEntry<TEntity, TDbContext, TService>(entity, context, service, exception, swallow),
		IUpdateFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class DeleteFailedEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service,
	Exception exception,
	Boolean swallow)
	: ChangeFailedEntry<TEntity, TDbContext, TService>(entity, context, service, exception, swallow),
		IDeleteFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class InsertedEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service)
	: Entry<TEntity, TDbContext, TService>(entity, context,
		service), IInsertedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class UpdatedEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service)
	: Entry<TEntity, TDbContext, TService>(entity, context,
		service), IUpdatedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class DeletedEntry<TEntity, TDbContext, TService>(
	TEntity entity,
	TDbContext context,
	IServiceProvider service)
	: Entry<TEntity, TDbContext, TService>(entity, context,
		service), IDeletedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

#endregion

#region Wrapped with generic service

internal class WrappedInsertingEntry<TEntity, TDbContext, TService>(IInsertingEntry<TEntity, TDbContext> entry)
	: WrappedBeforeEntry<IInsertingEntry<TEntity, TDbContext>, TEntity, TDbContext, TService>(entry),
		IInsertingEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class WrappedUpdatingEntry<TEntity, TDbContext, TService>(IUpdatingEntry<TEntity, TDbContext> entry)
	: WrappedBeforeChangeEntry<IUpdatingEntry<TEntity, TDbContext>, TEntity, TDbContext, TService>(entry),
		IUpdatingEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class WrappedDeletingEntry<TEntity, TDbContext, TService>(IDeletingEntry<TEntity, TDbContext> entry)
	: WrappedBeforeChangeEntry<IDeletingEntry<TEntity, TDbContext>, TEntity, TDbContext, TService>(entry),
		IDeletingEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class
	WrappedInsertFailedEntry<TEntity, TDbContext, TService>(IInsertFailedEntry<TEntity, TDbContext> entry)
	: WrappedFailedEntry<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext, TService>(entry),
		IInsertFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class
	WrappedUpdateFailedEntry<TEntity, TDbContext, TService>(IUpdateFailedEntry<TEntity, TDbContext> entry)
	: WrappedChangeFailedEntry<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext, TService>(entry),
		IUpdateFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class
	WrappedDeleteFailedEntry<TEntity, TDbContext, TService>(IDeleteFailedEntry<TEntity, TDbContext> entry)
	: WrappedChangeFailedEntry<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext, TService>(entry),
		IDeleteFailedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class WrappedInsertedEntry<TEntity, TDbContext, TService>(IInsertedEntry<TEntity, TDbContext> entry)
	: WrappedEntry<IInsertedEntry<TEntity, TDbContext>, TEntity, TDbContext, TService>(entry),
		IInsertedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class WrappedUpdatedEntry<TEntity, TDbContext, TService>(IUpdatedEntry<TEntity, TDbContext> entry)
	: WrappedEntry<IUpdatedEntry<TEntity, TDbContext>, TEntity, TDbContext, TService>(entry),
		IUpdatedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

internal class WrappedDeletedEntry<TEntity, TDbContext, TService>(IDeletedEntry<TEntity, TDbContext> entry)
	: WrappedEntry<IDeletedEntry<TEntity, TDbContext>, TEntity, TDbContext, TService>(entry),
		IDeletedEntry<TEntity, TDbContext, TService>
	where TEntity : class
	where TDbContext : DbContext;

#endregion

#endregion