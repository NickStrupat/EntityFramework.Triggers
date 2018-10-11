using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif
	#region Non-generic
	public interface IEntry
	{
		/// <summary>The entity</summary>
		Object Entity { get; }

		/// <summary>The DbContext instance</summary>
		DbContext Context { get; }

		/// <summary>The service provider, if one is provided</summary>
		IServiceProvider Service { get; }
	}

	public interface IBeforeEntry : IEntry {
		/// <summary>Get or set a value that marks the change to be cancelled if true</summary>
		Boolean Cancel { get; set; }
	}

	public interface IFailedEntry : IEntry {
		/// <summary>The exception raised by the attempted change</summary>
		Exception Exception { get; }

		/// <summary>Get or set a value that marks the exception to be caught and swallowed if true, or to be allowed to propagate if false</summary>
		Boolean Swallow { get; set; }
	}

	public interface IAfterEntry : IEntry {}
	public interface IChangeEntry : IEntry {}
	public interface IBeforeChangeEntry : IChangeEntry, IBeforeEntry {}
	public interface IChangeFailedEntry : IChangeEntry, IFailedEntry {}
	public interface IAfterChangeEntry  : IChangeEntry, IAfterEntry {}

	#region Specific to events
	public interface IInsertingEntry : IBeforeEntry {
		/// <summary>Get or set a value that marks the insertion to be cancelled if true (the entity will not be persisted)</summary>
		new Boolean Cancel { get; set; }
	}
	public interface IUpdatingEntry : IBeforeChangeEntry {
		/// <summary>Get or set a value that marks the update to be cancelled if true (changes to the entity properties will not be persisted)</summary>
		new Boolean Cancel { get; set; }
	}
	public interface IDeletingEntry : IBeforeChangeEntry {
		/// <summary>Get or set a value that marks the deletion to be cancelled if true (the entity will not be deleted, but changes to the entity properties will be persisted)</summary>
		new Boolean Cancel { get; set; }
	}

	public interface IInsertFailedEntry : IFailedEntry {}
	public interface IUpdateFailedEntry : IChangeFailedEntry {}
	public interface IDeleteFailedEntry : IChangeFailedEntry {}

	public interface IInsertedEntry : IAfterEntry {}
	public interface IUpdatedEntry  : IAfterChangeEntry {}
	public interface IDeletedEntry  : IAfterChangeEntry {}
	#endregion
	#endregion

	#region Without specific DbContext type
	/// <summary>Contains the context and the instance of the changed entity</summary>
	public interface IEntry<out TEntity> : IEntry where TEntity : class {
		/// <summary>The entity</summary>
		new TEntity Entity { get; }
	}

	public interface IBeforeEntry<out TEntity> : IBeforeEntry, IEntry<TEntity> where TEntity : class {}
	public interface IFailedEntry<out TEntity> : IFailedEntry, IEntry<TEntity> where TEntity : class {}
	public interface IAfterEntry<out TEntity> : IAfterEntry, IEntry<TEntity> where TEntity : class {}
	public interface IChangeEntry<out TEntity> : IChangeEntry, IEntry<TEntity> where TEntity : class {}
	public interface IBeforeChangeEntry<out TEntity> : IBeforeChangeEntry, IChangeEntry<TEntity>, IBeforeEntry<TEntity> where TEntity : class {
		/// <summary>An object representing the state of the entity before the changes</summary>
		TEntity Original { get; }
	}
	public interface IChangeFailedEntry<out TEntity> : IChangeFailedEntry, IChangeEntry<TEntity>, IFailedEntry<TEntity> where TEntity : class {}
	public interface IAfterChangeEntry <out TEntity> : IAfterChangeEntry, IChangeEntry<TEntity>, IAfterEntry <TEntity> where TEntity : class {}

	#region Specific to events
	public interface IInsertingEntry<out TEntity> : IInsertingEntry, IBeforeEntry<TEntity> where TEntity : class {}
	public interface IUpdatingEntry <out TEntity> : IUpdatingEntry, IBeforeChangeEntry<TEntity> where TEntity : class {}
	public interface IDeletingEntry <out TEntity> : IDeletingEntry, IBeforeChangeEntry<TEntity> where TEntity : class {}

	public interface IInsertFailedEntry<out TEntity> : IInsertFailedEntry, IFailedEntry<TEntity> where TEntity : class {}
	public interface IUpdateFailedEntry<out TEntity> : IUpdateFailedEntry, IChangeFailedEntry<TEntity> where TEntity : class {}
	public interface IDeleteFailedEntry<out TEntity> : IDeleteFailedEntry, IChangeFailedEntry<TEntity> where TEntity : class {}

	public interface IInsertedEntry<out TEntity> : IInsertedEntry, IAfterEntry<TEntity> where TEntity : class {}
	public interface IUpdatedEntry <out TEntity> : IUpdatedEntry, IAfterChangeEntry<TEntity> where TEntity : class {}
	public interface IDeletedEntry <out TEntity> : IDeletedEntry, IAfterChangeEntry<TEntity> where TEntity : class {}
	#endregion
	#endregion
	
	#region With specific DbContext type
	/// <inheritdoc />
	/// <summary>Contains the context and the instance of the changed entity</summary>
	public interface IEntry<out TEntity, out TDbContext> : IEntry<TEntity> where TEntity : class where TDbContext : DbContext {
		/// <summary>The TDbContext instance</summary>
		new TDbContext Context { get; }
	}

	public interface IBeforeEntry      <out TEntity, out TDbContext> : IBeforeEntry      <TEntity>, IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IFailedEntry      <out TEntity, out TDbContext> : IFailedEntry      <TEntity>, IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IAfterEntry       <out TEntity, out TDbContext> : IAfterEntry       <TEntity>, IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IChangeEntry      <out TEntity, out TDbContext> : IChangeEntry      <TEntity>, IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IBeforeChangeEntry<out TEntity, out TDbContext> : IBeforeChangeEntry<TEntity>, IChangeEntry<TEntity, TDbContext>, IBeforeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IChangeFailedEntry<out TEntity, out TDbContext> : IChangeFailedEntry<TEntity>, IChangeEntry<TEntity, TDbContext>, IFailedEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IAfterChangeEntry <out TEntity, out TDbContext> : IAfterChangeEntry <TEntity>, IChangeEntry<TEntity, TDbContext>, IAfterEntry <TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	
	#region Specific to events
	public interface IInsertingEntry<out TEntity, out TDbContext> : IInsertingEntry<TEntity>, IBeforeEntry      <TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IUpdatingEntry <out TEntity, out TDbContext> : IUpdatingEntry <TEntity>, IBeforeChangeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IDeletingEntry <out TEntity, out TDbContext> : IDeletingEntry <TEntity>, IBeforeChangeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}

	public interface IInsertFailedEntry<out TEntity, out TDbContext> : IInsertFailedEntry<TEntity>, IFailedEntry      <TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IUpdateFailedEntry<out TEntity, out TDbContext> : IUpdateFailedEntry<TEntity>, IChangeFailedEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IDeleteFailedEntry<out TEntity, out TDbContext> : IDeleteFailedEntry<TEntity>, IChangeFailedEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}

	public interface IInsertedEntry<out TEntity, out TDbContext> : IInsertedEntry<TEntity>, IAfterEntry      <TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IUpdatedEntry <out TEntity, out TDbContext> : IUpdatedEntry <TEntity>, IAfterChangeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	public interface IDeletedEntry <out TEntity, out TDbContext> : IDeletedEntry <TEntity>, IAfterChangeEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {}
	#endregion
	#endregion

	#region With generic service
	public interface IEntry<out TEntity, out TDbContext, out TService> : IEntry<TEntity, TDbContext> where TEntity : class where TDbContext : DbContext {
		/// <summary>The service object injected by the dependency injection container, if one is provided</summary>
		new TService Service { get; }
	}

	public interface IBeforeEntry      <out TEntity, out TDbContext, out TService> : IBeforeEntry      <TEntity, TDbContext>, IEntry<TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	public interface IFailedEntry      <out TEntity, out TDbContext, out TService> : IFailedEntry      <TEntity, TDbContext>, IEntry<TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	public interface IAfterEntry       <out TEntity, out TDbContext, out TService> : IAfterEntry       <TEntity, TDbContext>, IEntry<TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	public interface IChangeEntry      <out TEntity, out TDbContext, out TService> : IChangeEntry      <TEntity, TDbContext>, IEntry<TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	public interface IBeforeChangeEntry<out TEntity, out TDbContext, out TService> : IBeforeChangeEntry<TEntity, TDbContext>, IChangeEntry<TEntity, TDbContext, TService>, IBeforeEntry<TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	public interface IChangeFailedEntry<out TEntity, out TDbContext, out TService> : IChangeFailedEntry<TEntity, TDbContext>, IChangeEntry<TEntity, TDbContext, TService>, IFailedEntry<TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	public interface IAfterChangeEntry <out TEntity, out TDbContext, out TService> : IAfterChangeEntry <TEntity, TDbContext>, IChangeEntry<TEntity, TDbContext, TService>, IAfterEntry <TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	
	#region Specific to events
	public interface IInsertingEntry<out TEntity, out TDbContext, out TService> : IInsertingEntry<TEntity, TDbContext>, IBeforeEntry      <TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	public interface IUpdatingEntry <out TEntity, out TDbContext, out TService> : IUpdatingEntry <TEntity, TDbContext>, IBeforeChangeEntry<TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	public interface IDeletingEntry <out TEntity, out TDbContext, out TService> : IDeletingEntry <TEntity, TDbContext>, IBeforeChangeEntry<TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}

	public interface IInsertFailedEntry<out TEntity, out TDbContext, out TService> : IInsertFailedEntry<TEntity, TDbContext>, IFailedEntry      <TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	public interface IUpdateFailedEntry<out TEntity, out TDbContext, out TService> : IUpdateFailedEntry<TEntity, TDbContext>, IChangeFailedEntry<TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	public interface IDeleteFailedEntry<out TEntity, out TDbContext, out TService> : IDeleteFailedEntry<TEntity, TDbContext>, IChangeFailedEntry<TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}

	public interface IInsertedEntry<out TEntity, out TDbContext, out TService> : IInsertedEntry<TEntity, TDbContext>, IAfterEntry      <TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	public interface IUpdatedEntry <out TEntity, out TDbContext, out TService> : IUpdatedEntry <TEntity, TDbContext>, IAfterChangeEntry<TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	public interface IDeletedEntry <out TEntity, out TDbContext, out TService> : IDeletedEntry <TEntity, TDbContext>, IAfterChangeEntry<TEntity, TDbContext, TService> where TEntity : class where TDbContext : DbContext {}
	#endregion
	#endregion
}