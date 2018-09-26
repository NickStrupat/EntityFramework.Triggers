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
	public partial class TriggerEvent<TEntry, TEntity, TDbContext>
	where TEntry : IEntry<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		private readonly Func<IServiceProvider> getServiceProvider;
		private IServiceProvider serviceProvider => getServiceProvider() ?? throw new InvalidOperationException("Service provider must not be null");
		internal TriggerEvent(Func<IServiceProvider> getServiceProvider) => this.getServiceProvider = getServiceProvider;

		private TService S<TService>() => (TService)serviceProvider.GetService(typeof(TService));

		private ImmutableArray<WrappedHandler> wrappedHandlers = ImmutableArray<WrappedHandler>.Empty;
		
		internal void Raise(TEntry entry)
		{
			var latestWrappedHandlers = ImmutableInterlockedRead(ref wrappedHandlers);
			foreach (var wrappedHandler in latestWrappedHandlers)
				wrappedHandler.Invoke(entry);
		}

		private struct WrappedHandler : IEquatable<WrappedHandler>
		{
			private readonly Delegate source;
			private readonly Action<TEntry> wrapper;

			public WrappedHandler(Delegate source, Action<TEntry> wrapper)
			{
				this.source = source ?? throw new ArgumentNullException(nameof(source));
				this.wrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
			}

			public Boolean Equals(WrappedHandler other) => ReferenceEquals(source, other.source) || source == other.source;
			public override Boolean Equals(Object o) => o is WrappedHandler wrappedHandler && Equals(wrappedHandler);
			public override Int32 GetHashCode() => source.GetHashCode() ^ wrapper.GetHashCode();
			public void Invoke(TEntry entry) => wrapper.Invoke(entry);
		}

		private static void Add(ref ImmutableArray<WrappedHandler> wrappedHandlers, Delegate source, Action<TEntry> wrapper)
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
	}
}
