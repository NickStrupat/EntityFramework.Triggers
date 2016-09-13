using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif

	#region IEntry<TTriggerable, TDbContext>
	/// <summary>Contains the context and the instance of the changed entity</summary>
	public interface IEntry<out TEntity, out TDbContext> where TEntity : class {
		TEntity Entity { get; }
		TDbContext Context { get; }
	}

	public interface IBeforeEntry<out TEntity, out TDbContext> : IEntry<TEntity, TDbContext> where TEntity : class {
		void Cancel();
	}

	public interface IFailedEntry<out TEntity, out TDbContext> : IEntry<TEntity, TDbContext> where TEntity : class {
		Exception Exception { get; }
	}

	public interface IAfterEntry<out TEntity, out TDbContext> : IEntry<TEntity, TDbContext> where TEntity : class { }
	
	public interface IChangeEntry<out TEntity, out TDbContext> : IEntry<TEntity, TDbContext> where TEntity : class {
		/// <summary>A typed wrapper around DbEntityEntry.OriginalValues</summary>
		TEntity Original { get; }
	}

	public interface IBeforeChangeEntry<out TEntity, out TDbContext> : IChangeEntry<TEntity, TDbContext>, IBeforeEntry<TEntity, TDbContext> where TEntity : class { }
	public interface IChangeFailedEntry<out TEntity, out TDbContext> : IChangeEntry<TEntity, TDbContext>, IFailedEntry<TEntity, TDbContext> where TEntity : class { }
	public interface IAfterChangeEntry <out TEntity, out TDbContext> : IChangeEntry<TEntity, TDbContext>, IEntry<TEntity, TDbContext>       where TEntity : class { }
	#endregion
}