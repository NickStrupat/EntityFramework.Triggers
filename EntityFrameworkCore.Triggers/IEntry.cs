using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif

	/// <summary>Contains the context and the instance of the changed entity</summary>
	public interface IEntry<out TEntity> where TEntity : class {
		TEntity Entity { get; }
		DbContext Context { get; }
	}

	public interface IBeforeEntry<out TEntity> : IEntry<TEntity> where TEntity : class {
		void Cancel();
	}

	public interface IFailedEntry<out TEntity> : IEntry<TEntity> where TEntity : class {
		Exception Exception { get; }
	}

	public interface IAfterEntry<out TEntity> : IEntry<TEntity> where TEntity : class { }

	public interface IChangeEntry<out TEntity> : IEntry<TEntity> where TEntity : class {
		/// <summary>A typed wrapper around DbEntityEntry.OriginalValues</summary>
		TEntity Original { get; }
	}

	public interface IBeforeChangeEntry<out TEntity> : IChangeEntry<TEntity>, IBeforeEntry<TEntity> where TEntity : class { }
	public interface IChangeFailedEntry<out TEntity> : IChangeEntry<TEntity>, IFailedEntry<TEntity> where TEntity : class { }
	public interface IAfterChangeEntry <out TEntity> : IChangeEntry<TEntity>, IAfterEntry <TEntity> where TEntity : class { }

	/// <summary>Contains the context and the instance of the changed entity</summary>
	public interface IEntry<out TEntity, out TDbContext> : IEntry<TEntity> where TEntity : class where TDbContext : DbContext {
		new TDbContext Context { get; }
	}

	public interface IBeforeEntry      <out TEntity, out TDbContext> : IBeforeEntry      <TEntity>, IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IFailedEntry      <out TEntity, out TDbContext> : IFailedEntry      <TEntity>, IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IAfterEntry       <out TEntity, out TDbContext> : IAfterEntry       <TEntity>, IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IChangeEntry      <out TEntity, out TDbContext> : IChangeEntry      <TEntity>, IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IBeforeChangeEntry<out TEntity, out TDbContext> : IBeforeChangeEntry<TEntity>, IChangeEntry<TEntity, TDbContext>, IBeforeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	public interface IChangeFailedEntry<out TEntity, out TDbContext> : IChangeFailedEntry<TEntity>, IChangeEntry<TEntity, TDbContext>, IFailedEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	public interface IAfterChangeEntry <out TEntity, out TDbContext> : IAfterChangeEntry <TEntity>, IChangeEntry<TEntity, TDbContext>, IAfterEntry <TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
}