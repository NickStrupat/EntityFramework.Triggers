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
}