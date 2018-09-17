using System;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.Triggers
{
	public class Triggerzz<TEntity> where TEntity : class
	{
		private readonly IServiceProvider serviceProvider;
		public Triggerzz(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;
		private TService S<TService>() => serviceProvider.GetService<TService>();

		struct WrappedHandler<TEntry> : IEquatable<WrappedHandler<TEntry>> where TEntry : IEntry<TEntity>
		{
			private readonly Object source;
			private readonly Action<TEntry> wrapper;
			public WrappedHandler(Object source, Action<TEntry> wrapper) { this.source = source; this.wrapper = wrapper; }
			public Boolean Equals(WrappedHandler<TEntry> other) => ReferenceEquals(source, other.source);
			public void Invoke(TEntry entry) => wrapper.Invoke(entry);
		}

		private static void Add<TEntry>(ref ImmutableArray<WrappedHandler<TEntry>> array, Object source, Action<TEntry> wrapper) where TEntry : IEntry<TEntity>
		{
			ImmutableArray<WrappedHandler<TEntry>> initial, computed;
			do
			{
				initial = ImmutableInterlockedRead(ref array);
				computed = initial.Add(new WrappedHandler<TEntry>(source, wrapper));
			}
			while (initial != ImmutableInterlocked.InterlockedCompareExchange(ref array, computed, initial));
		}

		private static void Remove<TEntry>(ref ImmutableArray<WrappedHandler<TEntry>> array, Object source) where TEntry : IEntry<TEntity>
		{
			ImmutableArray<WrappedHandler<TEntry>> initial, computed;
			do
			{
				initial = ImmutableInterlockedRead(ref array);
				var index = initial.LastIndexOf(new WrappedHandler<TEntry>(source, null));
				if (index == -1)
					return;
				computed = initial.RemoveAt(index);
			}
			while (initial != ImmutableInterlocked.InterlockedCompareExchange(ref array, computed, initial));
		}

		private static ImmutableArray<WrappedHandler<TEntry>> ImmutableInterlockedRead<TEntry>(ref ImmutableArray<WrappedHandler<TEntry>> array) where TEntry : IEntry<TEntity> =>
			ImmutableInterlocked.InterlockedCompareExchange(ref array, ImmutableArray<WrappedHandler<TEntry>>.Empty, ImmutableArray<WrappedHandler<TEntry>>.Empty);

		private ImmutableArray<WrappedHandler<IInsertingEntry<TEntity>>> inserting = ImmutableArray<WrappedHandler<IInsertingEntry<TEntity>>>.Empty;

		public void InsertingAdd(Action<IInsertingEntry<TEntity>> handler) =>
			Add(ref inserting, handler, handler);

		public void InsertingAdd<TService>(Action<IInsertingEntry<TEntity>, TService> handler)
		{
			void Wrapper(IInsertingEntry<TEntity> e) => handler.Invoke(e, S<TService>());
			Add(ref inserting, handler, Wrapper);
		}

		public void InsertingAdd<TService1, TService2>(Action<IInsertingEntry<TEntity>, TService1, TService2> handler)
		{
			void Wrapper(IInsertingEntry<TEntity> e) => handler.Invoke(e, S<TService1>(), S<TService2>());
			Add(ref inserting, handler, Wrapper);
		}

		public void InsertingRemove<TService1, TService2>(Action<IInsertingEntry<TEntity>, TService1, TService2> handler)
		{
			Remove(ref inserting, handler);
		}
	}
}
