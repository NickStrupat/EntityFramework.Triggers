using System;
using System.Data.Entity;

namespace EntityFramework.Triggers {
	#region IEntry<TTriggerable, TDbContext>
	/// <summary>Contains the context and the instance of the changed entity</summary>
	public interface IEntry<out TTriggerable, out TDbContext> where TTriggerable : class, ITriggerable {
		TTriggerable Entity { get; }
		TDbContext Context { get; }
	}

	public interface IBeforeEntry<out TTriggerable, out TDbContext> : IEntry<TTriggerable, TDbContext> where TTriggerable : class, ITriggerable {
		void Cancel();
	}

	public interface IFailedEntry<out TTriggerable, out TDbContext> : IEntry<TTriggerable, TDbContext> where TTriggerable : class, ITriggerable {
		Exception Exception { get; }
	}

	public interface IAfterEntry<out TTriggerable, out TDbContext> : IEntry<TTriggerable, TDbContext> where TTriggerable : class, ITriggerable { }
	
	public interface IChangeEntry<out TTriggerable, out TDbContext> : IEntry<TTriggerable, TDbContext> where TTriggerable : class, ITriggerable {
		/// <summary>A typed wrapper around DbEntityEntry.OriginalValues</summary>
		TTriggerable Original { get; }
	}

	public interface IBeforeChangeEntry<out TTriggerable, out TDbContext> : IChangeEntry<TTriggerable, TDbContext>, IBeforeEntry<TTriggerable, TDbContext> where TTriggerable : class, ITriggerable { }
	public interface IChangeFailedEntry<out TTriggerable, out TDbContext> : IChangeEntry<TTriggerable, TDbContext>, IFailedEntry<TTriggerable, TDbContext> where TTriggerable : class, ITriggerable { }
	public interface IAfterChangeEntry <out TTriggerable, out TDbContext> : IChangeEntry<TTriggerable, TDbContext>, IEntry<TTriggerable, TDbContext>       where TTriggerable : class, ITriggerable { }
	#endregion
	#region IEntry<TTriggerable>
	public interface IEntry      <out TTriggerable> : IEntry      <TTriggerable, DbContext> where TTriggerable : class, ITriggerable { }
	public interface IBeforeEntry<out TTriggerable> : IBeforeEntry<TTriggerable, DbContext>, IEntry<TTriggerable> where TTriggerable : class, ITriggerable { }
	public interface IFailedEntry<out TTriggerable> : IFailedEntry<TTriggerable, DbContext>, IEntry<TTriggerable> where TTriggerable : class, ITriggerable { }
	public interface IAfterEntry <out TTriggerable> : IAfterEntry <TTriggerable, DbContext>, IEntry<TTriggerable> where TTriggerable : class, ITriggerable { }
	public interface IChangeEntry<out TTriggerable> : IChangeEntry<TTriggerable, DbContext>, IEntry<TTriggerable> where TTriggerable : class, ITriggerable { }

	public interface IBeforeChangeEntry<out TTriggerable> : IChangeEntry<TTriggerable>, IBeforeEntry<TTriggerable> where TTriggerable : class, ITriggerable { }
	public interface IChangeFailedEntry<out TTriggerable> : IChangeEntry<TTriggerable>, IFailedEntry<TTriggerable> where TTriggerable : class, ITriggerable { }
	public interface IAfterChangeEntry <out TTriggerable> : IChangeEntry<TTriggerable>, IEntry      <TTriggerable> where TTriggerable : class, ITriggerable { }
	#endregion
}