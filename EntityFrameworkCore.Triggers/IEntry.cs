using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif
	#region Without specific DbContext type
	/// <summary>Contains the context and the instance of the changed entity</summary>
	public interface IEntry<out TEntity> where TEntity : class {
		/// <summary>The entity</summary>
		TEntity Entity { get; }

		/// <summary>The DbContext instance</summary>
		DbContext Context { get; }
	}

	public interface IBeforeEntry<out TEntity> : IEntry<TEntity> where TEntity : class {
		/// <summary>Get or set a value that marks the cancellation of the change</summary>
		Boolean Cancel { get; set; }
	}

	public interface IFailedEntry<out TEntity> : IEntry<TEntity> where TEntity : class {
		/// <summary>The exception raised by the attempted change</summary>
		Exception Exception { get; }

		/// <summary>Get or set a value that marks the exception to be caught and swallowed if true, or to be allowed to propagate if false</summary>
		Boolean Swallow { get; set; }
	}

	public interface IAfterEntry<out TEntity> : IEntry<TEntity> where TEntity : class { }

	public interface IChangeEntry<out TEntity> : IEntry<TEntity> where TEntity : class {}

	public interface IBeforeChangeEntry<out TEntity> : IChangeEntry<TEntity>, IBeforeEntry<TEntity> where TEntity : class {
		/// <summary>A proxy object representing the state of the entity before the changes</summary>
		TEntity Original { get; }
	}
	public interface IChangeFailedEntry<out TEntity> : IChangeEntry<TEntity>, IFailedEntry<TEntity> where TEntity : class { }
	public interface IAfterChangeEntry <out TEntity> : IChangeEntry<TEntity>, IAfterEntry <TEntity> where TEntity : class { }

	#region Specific to events
	public interface IInsertingEntry<out TEntity> : IBeforeEntry<TEntity> where TEntity : class { }
	public interface IUpdatingEntry <out TEntity> : IBeforeChangeEntry<TEntity> where TEntity : class { }
	public interface IDeletingEntry <out TEntity> : IBeforeChangeEntry<TEntity> where TEntity : class { }

	public interface IInsertFailedEntry<out TEntity> : IFailedEntry<TEntity> where TEntity : class { }
	public interface IUpdateFailedEntry<out TEntity> : IChangeFailedEntry<TEntity> where TEntity : class { }
	public interface IDeleteFailedEntry<out TEntity> : IChangeFailedEntry<TEntity> where TEntity : class { }

	public interface IInsertedEntry<out TEntity> : IAfterEntry<TEntity> where TEntity : class { }
	public interface IUpdatedEntry <out TEntity> : IAfterChangeEntry<TEntity> where TEntity : class { }
	public interface IDeletedEntry <out TEntity> : IAfterChangeEntry<TEntity> where TEntity : class { }
	#endregion
	#endregion

	#region With specific DbContext type
	/// <summary>Contains the context and the instance of the changed entity</summary>
	public interface IEntry<out TEntity, out TDbContext> : IEntry<TEntity> where TEntity : class where TDbContext : DbContext {
		/// <summary>The TDbContext instance</summary>
		new TDbContext Context { get; }
	}

	public interface IBeforeEntry      <out TEntity, out TDbContext> : IBeforeEntry      <TEntity>, IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IFailedEntry      <out TEntity, out TDbContext> : IFailedEntry      <TEntity>, IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IAfterEntry       <out TEntity, out TDbContext> : IAfterEntry       <TEntity>, IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IChangeEntry      <out TEntity, out TDbContext> : IChangeEntry      <TEntity>, IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IBeforeChangeEntry<out TEntity, out TDbContext> : IBeforeChangeEntry<TEntity>, IChangeEntry<TEntity, TDbContext>, IBeforeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	public interface IChangeFailedEntry<out TEntity, out TDbContext> : IChangeFailedEntry<TEntity>, IChangeEntry<TEntity, TDbContext>, IFailedEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	public interface IAfterChangeEntry <out TEntity, out TDbContext> : IAfterChangeEntry <TEntity>, IChangeEntry<TEntity, TDbContext>, IAfterEntry <TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	
	#region Specific to events
	public interface IInsertingEntry<out TEntity, out TDbContext> : IInsertingEntry<TEntity>, IBeforeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	public interface IUpdatingEntry <out TEntity, out TDbContext> : IUpdatingEntry <TEntity>, IBeforeChangeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	public interface IDeletingEntry <out TEntity, out TDbContext> : IDeletingEntry <TEntity>, IBeforeChangeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }

	public interface IInsertFailedEntry<out TEntity, out TDbContext> : IInsertFailedEntry<TEntity>, IFailedEntry      <TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	public interface IUpdateFailedEntry<out TEntity, out TDbContext> : IUpdateFailedEntry<TEntity>, IChangeFailedEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	public interface IDeleteFailedEntry<out TEntity, out TDbContext> : IDeleteFailedEntry<TEntity>, IChangeFailedEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }

	public interface IInsertedEntry<out TEntity, out TDbContext> : IInsertedEntry<TEntity>, IAfterEntry      <TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	public interface IUpdatedEntry <out TEntity, out TDbContext> : IUpdatedEntry <TEntity>, IAfterChangeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	public interface IDeletedEntry <out TEntity, out TDbContext> : IDeletedEntry <TEntity>, IAfterChangeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext { }
	#endregion
	#endregion
}