using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif
	
	/// <summary>Contains the context and the instance of the changed entity</summary>
	public interface IEntry<out TEntity, out TDbContext> where TEntity : class where TDbContext : DbContext {
		TEntity Entity { get; }
		TDbContext Context { get; }
	}

	public interface IBeforeEntry<out TEntity, out TDbContext> : IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {
		void Cancel();
	}

	public interface IFailedEntry<out TEntity, out TDbContext> : IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {
		Exception Exception { get; }
	}

	public interface IAfterEntry<out TEntity, out TDbContext> : IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	
	public interface IChangeEntry<out TEntity, out TDbContext> : IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {
		/// <summary>A typed wrapper around DbEntityEntry.OriginalValues</summary>
		TEntity Original { get; }
	}

	public interface IBeforeChangeEntry<out TEntity, out TDbContext> : IChangeEntry<TEntity, TDbContext>, IBeforeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	public interface IChangeFailedEntry<out TEntity, out TDbContext> : IChangeEntry<TEntity, TDbContext>, IFailedEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	public interface IAfterChangeEntry <out TEntity, out TDbContext> : IChangeEntry<TEntity, TDbContext>, IEntry<TEntity, TDbContext>       where TEntity : class where TDbContext : DbContext { }
}