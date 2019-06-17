using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	internal abstract class TriggerEvent : ITriggerEvent, ITriggerEventInternal
	{
		public void Raise(Object entry) => RaiseInternal(entry);
		public Task RaiseAsync(Object entry) => RaiseInternalAsync(entry);

		protected abstract void RaiseInternal(Object entry);
		protected abstract Task RaiseInternalAsync(Object entry);
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

		protected sealed override async Task RaiseInternalAsync(Object entry)
		{
			var x = (TEntry)entry;
			var latestWrappedHandlers = ImmutableInterlockedRead(ref wrappedHandlers);
			foreach (var wrappedHandler in latestWrappedHandlers)
				await wrappedHandler.InvokeAsync(x);
		}

		protected struct WrappedHandler : IEquatable<WrappedHandler>
		{
			private readonly Delegate source;
			private readonly DelegateSynchronyUnion<TEntry> wrapper;

			public WrappedHandler(Delegate source, DelegateSynchronyUnion<TEntry> wrapper)
			{
				this.source = source ?? throw new ArgumentNullException(nameof(source));
				this.wrapper = wrapper;
			}

			public Boolean Equals(WrappedHandler other) => ReferenceEquals(source, other.source) || source == other.source;
			public override Boolean Equals(Object o) => o is WrappedHandler wrappedHandler && Equals(wrappedHandler);
			public override Int32 GetHashCode() => source.GetHashCode() ^ wrapper.GetHashCode();
			public void Invoke(TEntry entry) => wrapper.Invoke(entry);
			public Task InvokeAsync(TEntry entry) => wrapper.InvokeAsync(entry);
		}

		protected static void Add(ref ImmutableArray<WrappedHandler> wrappedHandlers, Delegate source, DelegateSynchronyUnion<TEntry> wrapper)
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
				var index = initial.LastIndexOf(new WrappedHandler(source, default));
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
			Add(ref wrappedHandlers, handler, new DelegateSynchronyUnion<TEntry>(handler));
		}

		public void Remove(Action<TEntry> handler)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));
			Remove(ref wrappedHandlers, handler);
		}

		public void Add(Func<TEntry, Task> handler)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));
			Add(ref wrappedHandlers, handler, new DelegateSynchronyUnion<TEntry>(handler));
		}

		public void Remove(Func<TEntry, Task> handler)
		{
			if (handler == null)
				throw new ArgumentNullException(nameof(handler));
			Remove(ref wrappedHandlers, handler);
		}
	}
}
