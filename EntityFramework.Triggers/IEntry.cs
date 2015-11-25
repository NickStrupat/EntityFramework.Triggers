using System;
using System.Data.Entity;

namespace EntityFramework.Triggers {
	/// <summary>Contains the context and the instance of the changed entity</summary>
	public interface IEntry<out TTriggerable> where TTriggerable : class, ITriggerable {
		TTriggerable Entity { get; }
		DbContext Context { get; }
	}

	public interface IBeforeEntry<out TTriggerable> : IEntry<TTriggerable> where TTriggerable : class, ITriggerable {
		void Cancel();
	}

	public interface IFailedEntry<out TTriggerable> : IEntry<TTriggerable> where TTriggerable : class, ITriggerable {
		Exception Exception { get; }
	}

	public interface IAfterEntry<out TTriggerable> : IEntry<TTriggerable> where TTriggerable : class, ITriggerable {}



	public interface IChangeEntry<out TTriggerable> : IEntry<TTriggerable> where TTriggerable : class, ITriggerable {
		/// <summary>A typed wrapper around DbEntityEntry.OriginalValues</summary>
		TTriggerable Original { get; }
	}

	public interface IBeforeChangeEntry<out TTriggerable> : IChangeEntry<TTriggerable>, IBeforeEntry<TTriggerable> where TTriggerable : class, ITriggerable {}
	public interface IChangeFailedEntry<out TTriggerable> : IChangeEntry<TTriggerable>, IFailedEntry<TTriggerable> where TTriggerable : class, ITriggerable {}
	public interface IAfterChangeEntry<out TTriggerable> : IChangeEntry<TTriggerable>, IEntry<TTriggerable> where TTriggerable : class, ITriggerable {}
}