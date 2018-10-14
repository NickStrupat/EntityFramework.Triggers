using System;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	public interface IInsertingTriggerEvent<out TEntity, out TDbContext>
	: ITriggerEvent<IInsertingEntry<TEntity, TDbContext>, TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		void Add<TService>(Action<IInsertingEntry<TEntity, TDbContext, TService>> handler);
		void Remove<TService>(Action<IInsertingEntry<TEntity, TDbContext, TService>> handler);
	}

	internal class InsertingTriggerEvent<TEntity, TDbContext>
	: TriggerEvent<IInsertingEntry<TEntity, TDbContext>, TEntity, TDbContext>
	, IInsertingTriggerEvent<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public void Add<TService>(Action<IInsertingEntry<TEntity, TDbContext, TService>> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(new WrappedInsertingEntry<TEntity, TDbContext, TService>(entry)));

		public void Remove<TService>(Action<IInsertingEntry<TEntity, TDbContext, TService>> handler) =>
			Remove(ref wrappedHandlers, handler);
	}

	public interface IInsertFailedTriggerEvent<out TEntity, out TDbContext>
	: ITriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		void Add<TService>(Action<IInsertFailedEntry<TEntity, TDbContext, TService>> handler);
		void Remove<TService>(Action<IInsertFailedEntry<TEntity, TDbContext, TService>> handler);
	}

	internal class InsertFailedTriggerEvent<TEntity, TDbContext>
	: TriggerEvent<IInsertFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>
	, IInsertFailedTriggerEvent<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public void Add<TService>(Action<IInsertFailedEntry<TEntity, TDbContext, TService>> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(new WrappedInsertFailedEntry<TEntity, TDbContext, TService>(entry)));

		public void Remove<TService>(Action<IInsertFailedEntry<TEntity, TDbContext, TService>> handler) =>
			Remove(ref wrappedHandlers, handler);
	}

	public interface IInsertedTriggerEvent<out TEntity, out TDbContext>
	: ITriggerEvent<IInsertedEntry<TEntity, TDbContext>, TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		void Add<TService>(Action<IInsertedEntry<TEntity, TDbContext, TService>> handler);
		void Remove<TService>(Action<IInsertedEntry<TEntity, TDbContext, TService>> handler);
	}

	internal class InsertedTriggerEvent<TEntity, TDbContext>
	: TriggerEvent<IInsertedEntry<TEntity, TDbContext>, TEntity, TDbContext>
	, IInsertedTriggerEvent<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public void Add<TService>(Action<IInsertedEntry<TEntity, TDbContext, TService>> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(new WrappedInsertedEntry<TEntity, TDbContext, TService>(entry)));

		public void Remove<TService>(Action<IInsertedEntry<TEntity, TDbContext, TService>> handler) =>
			Remove(ref wrappedHandlers, handler);
	}

	public interface IDeletingTriggerEvent<out TEntity, out TDbContext>
	: ITriggerEvent<IDeletingEntry<TEntity, TDbContext>, TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		void Add<TService>(Action<IDeletingEntry<TEntity, TDbContext, TService>> handler);
		void Remove<TService>(Action<IDeletingEntry<TEntity, TDbContext, TService>> handler);
	}

	internal class DeletingTriggerEvent<TEntity, TDbContext>
	: TriggerEvent<IDeletingEntry<TEntity, TDbContext>, TEntity, TDbContext>
	, IDeletingTriggerEvent<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public void Add<TService>(Action<IDeletingEntry<TEntity, TDbContext, TService>> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(new WrappedDeletingEntry<TEntity, TDbContext, TService>(entry)));

		public void Remove<TService>(Action<IDeletingEntry<TEntity, TDbContext, TService>> handler) =>
			Remove(ref wrappedHandlers, handler);
	}

	public interface IDeleteFailedTriggerEvent<out TEntity, out TDbContext>
	: ITriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		void Add<TService>(Action<IDeleteFailedEntry<TEntity, TDbContext, TService>> handler);
		void Remove<TService>(Action<IDeleteFailedEntry<TEntity, TDbContext, TService>> handler);
	}

	internal class DeleteFailedTriggerEvent<TEntity, TDbContext>
	: TriggerEvent<IDeleteFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>
	, IDeleteFailedTriggerEvent<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public void Add<TService>(Action<IDeleteFailedEntry<TEntity, TDbContext, TService>> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(new WrappedDeleteFailedEntry<TEntity, TDbContext, TService>(entry)));

		public void Remove<TService>(Action<IDeleteFailedEntry<TEntity, TDbContext, TService>> handler) =>
			Remove(ref wrappedHandlers, handler);
	}

	public interface IDeletedTriggerEvent<out TEntity, out TDbContext>
	: ITriggerEvent<IDeletedEntry<TEntity, TDbContext>, TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		void Add<TService>(Action<IDeletedEntry<TEntity, TDbContext, TService>> handler);
		void Remove<TService>(Action<IDeletedEntry<TEntity, TDbContext, TService>> handler);
	}

	internal class DeletedTriggerEvent<TEntity, TDbContext>
	: TriggerEvent<IDeletedEntry<TEntity, TDbContext>, TEntity, TDbContext>
	, IDeletedTriggerEvent<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public void Add<TService>(Action<IDeletedEntry<TEntity, TDbContext, TService>> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(new WrappedDeletedEntry<TEntity, TDbContext, TService>(entry)));

		public void Remove<TService>(Action<IDeletedEntry<TEntity, TDbContext, TService>> handler) =>
			Remove(ref wrappedHandlers, handler);
	}

	public interface IUpdatingTriggerEvent<out TEntity, out TDbContext>
	: ITriggerEvent<IUpdatingEntry<TEntity, TDbContext>, TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		void Add<TService>(Action<IUpdatingEntry<TEntity, TDbContext, TService>> handler);
		void Remove<TService>(Action<IUpdatingEntry<TEntity, TDbContext, TService>> handler);
	}

	internal class UpdatingTriggerEvent<TEntity, TDbContext>
	: TriggerEvent<IUpdatingEntry<TEntity, TDbContext>, TEntity, TDbContext>
	, IUpdatingTriggerEvent<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public void Add<TService>(Action<IUpdatingEntry<TEntity, TDbContext, TService>> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(new WrappedUpdatingEntry<TEntity, TDbContext, TService>(entry)));

		public void Remove<TService>(Action<IUpdatingEntry<TEntity, TDbContext, TService>> handler) =>
			Remove(ref wrappedHandlers, handler);
	}

	public interface IUpdateFailedTriggerEvent<out TEntity, out TDbContext>
	: ITriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		void Add<TService>(Action<IUpdateFailedEntry<TEntity, TDbContext, TService>> handler);
		void Remove<TService>(Action<IUpdateFailedEntry<TEntity, TDbContext, TService>> handler);
	}

	internal class UpdateFailedTriggerEvent<TEntity, TDbContext>
	: TriggerEvent<IUpdateFailedEntry<TEntity, TDbContext>, TEntity, TDbContext>
	, IUpdateFailedTriggerEvent<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public void Add<TService>(Action<IUpdateFailedEntry<TEntity, TDbContext, TService>> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(new WrappedUpdateFailedEntry<TEntity, TDbContext, TService>(entry)));

		public void Remove<TService>(Action<IUpdateFailedEntry<TEntity, TDbContext, TService>> handler) =>
			Remove(ref wrappedHandlers, handler);
	}

	public interface IUpdatedTriggerEvent<out TEntity, out TDbContext>
	: ITriggerEvent<IUpdatedEntry<TEntity, TDbContext>, TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		void Add<TService>(Action<IUpdatedEntry<TEntity, TDbContext, TService>> handler);
		void Remove<TService>(Action<IUpdatedEntry<TEntity, TDbContext, TService>> handler);
	}

	internal class UpdatedTriggerEvent<TEntity, TDbContext>
	: TriggerEvent<IUpdatedEntry<TEntity, TDbContext>, TEntity, TDbContext>
	, IUpdatedTriggerEvent<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		public void Add<TService>(Action<IUpdatedEntry<TEntity, TDbContext, TService>> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(new WrappedUpdatedEntry<TEntity, TDbContext, TService>(entry)));

		public void Remove<TService>(Action<IUpdatedEntry<TEntity, TDbContext, TService>> handler) =>
			Remove(ref wrappedHandlers, handler);
	}

}