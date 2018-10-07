using System;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
	public abstract class TriggerEvent : ITriggerEvent
	{
		public void Raise(Object entry, IServiceProvider serviceProvider) => RaiseInternal(entry, serviceProvider);
		
		protected abstract void RaiseInternal(Object entry, IServiceProvider serviceProvider);
	}

	public sealed partial class TriggerEvent<TEntry, TEntity, TDbContext>
	: TriggerEvent
	, IEquatable<TriggerEvent<TEntry, TEntity, TDbContext>>
	where TEntry : IEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		internal TriggerEvent() {}

		private static TService S<TService>(IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<TService>();

		private ImmutableArray<WrappedHandler> wrappedHandlers = ImmutableArray<WrappedHandler>.Empty;
		
		protected override void RaiseInternal(Object entry, IServiceProvider serviceProvider)
		{
			var x = (TEntry) entry;
			var latestWrappedHandlers = ImmutableInterlockedRead(ref wrappedHandlers);
			foreach (var wrappedHandler in latestWrappedHandlers)
				wrappedHandler.Invoke(x, serviceProvider);
		}

		private struct WrappedHandler : IEquatable<WrappedHandler>
		{
			private readonly Delegate source;
			private readonly Action<TEntry, IServiceProvider> wrapper;

			public WrappedHandler(Delegate source, Action<TEntry, IServiceProvider> wrapper)
			{
				this.source = source ?? throw new ArgumentNullException(nameof(source));
				this.wrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
			}

			public Boolean Equals(WrappedHandler other) => ReferenceEquals(source, other.source) || source == other.source;
			public override Boolean Equals(Object o) => o is WrappedHandler wrappedHandler && Equals(wrappedHandler);
			public override Int32 GetHashCode() => source.GetHashCode() ^ wrapper.GetHashCode();
			public void Invoke(TEntry entry, IServiceProvider serviceProvider) => wrapper.Invoke(entry, serviceProvider);
		}

		private static void Add(ref ImmutableArray<WrappedHandler> wrappedHandlers, Delegate source, Action<TEntry, IServiceProvider> wrapper)
		{
			ImmutableArray<WrappedHandler> initial, computed;
			do
			{
				initial = ImmutableInterlockedRead(ref wrappedHandlers);
				computed = initial.Add(new WrappedHandler(source, wrapper));
			}
			while (initial != ImmutableInterlocked.InterlockedCompareExchange(ref wrappedHandlers, computed, initial));
		}

		private static void Remove(ref ImmutableArray<WrappedHandler> wrappedHandlers, Delegate source)
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
	        void Wrapper(TEntry entry, IServiceProvider provider) => handler(entry);
			Add(ref wrappedHandlers, handler, Wrapper);
        }

		public void Remove(Action<TEntry> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}
	}
}
