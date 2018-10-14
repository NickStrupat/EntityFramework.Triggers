using System;
using System.Collections.Immutable;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	internal abstract class TriggerEvent : ITriggerEvent
	{
		public void Raise(Object entry) => RaiseInternal(entry);

		protected abstract void RaiseInternal(Object entry);
	}

	internal class TriggerEvent<TEntry, TEntity, TDbContext>
	: TriggerEvent
	, ITriggerEvent<TEntry, TEntity, TDbContext>
	, IEquatable<TriggerEvent<TEntry, TEntity, TDbContext>>
	where TEntry : IEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		internal TriggerEvent() {}

		private protected ImmutableArray<WrappedHandler> wrappedHandlers = ImmutableArray<WrappedHandler>.Empty;

		protected sealed override void RaiseInternal(Object entry)
		{
			var x = (TEntry)entry;
			var latestWrappedHandlers = ImmutableInterlockedRead(ref wrappedHandlers);
			foreach (var wrappedHandler in latestWrappedHandlers)
				wrappedHandler.Invoke(x);
		}

		protected struct WrappedHandler : IEquatable<WrappedHandler>
		{
			private readonly Delegate source;
			private readonly Action<TEntry> wrapper;

			public WrappedHandler(Delegate source, Action<TEntry> wrapper)
			{
				this.source = source ?? throw new ArgumentNullException(nameof(source));
				this.wrapper = wrapper;
			}

			public Boolean Equals(WrappedHandler other) => ReferenceEquals(source, other.source) || source == other.source;
			public override Boolean Equals(Object o) => o is WrappedHandler wrappedHandler && Equals(wrappedHandler);
			public override Int32 GetHashCode() => source.GetHashCode() ^ wrapper?.GetHashCode() ?? 0;
			public void Invoke(TEntry entry) => wrapper.Invoke(entry);
		}

		protected static void Add(ref ImmutableArray<WrappedHandler> wrappedHandlers, Delegate source, Action<TEntry> wrapper)
		{
			ImmutableArray<WrappedHandler> initial, computed;
			do
			{
				initial = ImmutableInterlockedRead(ref wrappedHandlers);
				computed = initial.Add(new WrappedHandler(source, wrapper));
			}
			while (initial != ImmutableInterlocked.InterlockedCompareExchange(ref wrappedHandlers, computed, initial));
		}

		protected static void Remove(ref ImmutableArray<WrappedHandler> wrappedHandlers, Delegate source)
		{
			ImmutableArray<WrappedHandler> initial, computed;
			do
			{
				initial = ImmutableInterlockedRead(ref wrappedHandlers);
				var index = initial.LastIndexOf(new WrappedHandler(source, null));
				if (index == -1)
					return;
				computed = initial.RemoveAt(index);
			}
			while (initial != ImmutableInterlocked.InterlockedCompareExchange(ref wrappedHandlers, computed, initial));
		}

		private static ImmutableArray<WrappedHandler> ImmutableInterlockedRead(ref ImmutableArray<WrappedHandler> array) =>
			ImmutableInterlocked.InterlockedCompareExchange(ref array, ImmutableArray<WrappedHandler>.Empty, ImmutableArray<WrappedHandler>.Empty);

		public override Boolean Equals(Object obj) => obj is TriggerEvent<TEntry, TEntity, TDbContext> triggerEvent && Equals(triggerEvent);
		public Boolean Equals(TriggerEvent<TEntry, TEntity, TDbContext> other) => other != null && ImmutableInterlockedRead(ref wrappedHandlers).Equals(ImmutableInterlockedRead(ref other.wrappedHandlers));
		public override Int32 GetHashCode() => ImmutableInterlockedRead(ref wrappedHandlers).GetHashCode();

		public void Add(Action<TEntry> handler)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));
			Add(ref wrappedHandlers, handler, handler);
		}

		public void Remove(Action<TEntry> handler)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));
			Remove(ref wrappedHandlers, handler);
		}
	}
}
