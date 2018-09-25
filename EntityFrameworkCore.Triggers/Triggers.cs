using System;
using System.Collections.Immutable;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif
	internal interface ITriggers<in TEntity, in TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		void RaiseInserting(IInsertingEntry<TEntity, TDbContext> entry);
		void RaiseInsertFailed(IInsertFailedEntry<TEntity, TDbContext> entry);
		void RaiseInserted(IInsertedEntry<TEntity, TDbContext> entry);
		void RaiseDeleting(IDeletingEntry<TEntity, TDbContext> entry);
		void RaiseDeleteFailed(IDeleteFailedEntry<TEntity, TDbContext> entry);
		void RaiseDeleted(IDeletedEntry<TEntity, TDbContext> entry);
		void RaiseUpdating(IUpdatingEntry<TEntity, TDbContext> entry);
		void RaiseUpdateFailed(IUpdateFailedEntry<TEntity, TDbContext> entry);
		void RaiseUpdated(IUpdatedEntry<TEntity, TDbContext> entry);
	}

	public class Triggers<TEntity, TDbContext> : ITriggers<TEntity, TDbContext>
	where TEntity : class
	where TDbContext : DbContext
	{
		private readonly IServiceProvider serviceProvider;
		public Triggers(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;
		private TService S<TService>() => (TService)serviceProvider.GetService(typeof(TService));

		struct WrappedHandler<TEntry> : IEquatable<WrappedHandler<TEntry>> where TEntry : IEntry<TEntity, TDbContext>
		{
			private readonly Object source;
			private readonly Action<TEntry> wrapper;
			public WrappedHandler(Object source, Action<TEntry> wrapper) { this.source = source; this.wrapper = wrapper; }
			public Boolean Equals(WrappedHandler<TEntry> other) => ReferenceEquals(source, other.source);
			public void Invoke(TEntry entry) => wrapper.Invoke(entry);
		}

		private static void Add<TEntry>(ref ImmutableArray<WrappedHandler<TEntry>> array, Object source, Action<TEntry> wrapper) where TEntry : IEntry<TEntity, TDbContext>
		{
			ImmutableArray<WrappedHandler<TEntry>> initial, computed;
			do
			{
				initial = ImmutableInterlockedRead(ref array);
				computed = initial.Add(new WrappedHandler<TEntry>(source, wrapper));
			}
			while (initial != ImmutableInterlocked.InterlockedCompareExchange(ref array, computed, initial));
		}

		private static void Remove<TEntry>(ref ImmutableArray<WrappedHandler<TEntry>> array, Object source) where TEntry : IEntry<TEntity, TDbContext>
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

		private static ImmutableArray<WrappedHandler<TEntry>> ImmutableInterlockedRead<TEntry>(ref ImmutableArray<WrappedHandler<TEntry>> array) where TEntry : IEntry<TEntity, TDbContext> =>
			ImmutableInterlocked.InterlockedCompareExchange(ref array, ImmutableArray<WrappedHandler<TEntry>>.Empty, ImmutableArray<WrappedHandler<TEntry>>.Empty);

		private ImmutableArray<WrappedHandler<IInsertingEntry<TEntity, TDbContext>>> inserting = ImmutableArray<WrappedHandler<IInsertingEntry<TEntity, TDbContext>>>.Empty;
		
		void ITriggers<TEntity, TDbContext>.RaiseInserting(IInsertingEntry<TEntity, TDbContext> entry)
		{
			var latestHandlers = ImmutableInterlockedRead(ref inserting);
			for (var i = 0; i < latestHandlers.Length; i++)
				latestHandlers[i].Invoke(entry);
		}
		
		public void InsertingAdd(Action<IInsertingEntry<TEntity, TDbContext>> handler) =>
			Add(ref inserting, handler, handler);

		public void InsertingAdd<TService>(Action<IInsertingEntry<TEntity, TDbContext>, TService> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService>()));

		public void InsertingRemove<TService>(Action<IInsertingEntry<TEntity, TDbContext>, TService> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void InsertingRemove<TService1, TService2>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>()));

		public void InsertingRemove<TService1, TService2, TService3>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3, TService4>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>()));

		public void InsertingRemove<TService1, TService2, TService3, TService4>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3, TService4, TService5>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>()));

		public void InsertingRemove<TService1, TService2, TService3, TService4, TService5>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>()));

		public void InsertingRemove<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>()));

		public void InsertingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>()));

		public void InsertingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>()));

		public void InsertingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>()));

		public void InsertingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>()));

		public void InsertingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>()));

		public void InsertingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>()));

		public void InsertingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>()));

		public void InsertingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>(), S<TService15>()));

		public void InsertingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IInsertingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Remove(ref inserting, handler);


		private ImmutableArray<WrappedHandler<IInsertFailedEntry<TEntity, TDbContext>>> insertFailed = ImmutableArray<WrappedHandler<IInsertFailedEntry<TEntity, TDbContext>>>.Empty;
		
		void ITriggers<TEntity, TDbContext>.RaiseInsertFailed(IInsertFailedEntry<TEntity, TDbContext> entry)
		{
			var latestHandlers = ImmutableInterlockedRead(ref insertFailed);
			for (var i = 0; i < latestHandlers.Length; i++)
				latestHandlers[i].Invoke(entry);
		}
		
		public void InsertFailedAdd(Action<IInsertFailedEntry<TEntity, TDbContext>> handler) =>
			Add(ref insertFailed, handler, handler);

		public void InsertFailedAdd<TService>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService>()));

		public void InsertFailedRemove<TService>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void InsertFailedRemove<TService1, TService2>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>()));

		public void InsertFailedRemove<TService1, TService2, TService3>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3, TService4>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>()));

		public void InsertFailedRemove<TService1, TService2, TService3, TService4>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3, TService4, TService5>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>()));

		public void InsertFailedRemove<TService1, TService2, TService3, TService4, TService5>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>()));

		public void InsertFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>()));

		public void InsertFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>()));

		public void InsertFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>()));

		public void InsertFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>()));

		public void InsertFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>()));

		public void InsertFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>()));

		public void InsertFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>()));

		public void InsertFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>()));

		public void InsertFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>(), S<TService15>()));

		public void InsertFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IInsertFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Remove(ref insertFailed, handler);


		private ImmutableArray<WrappedHandler<IInsertedEntry<TEntity, TDbContext>>> inserted = ImmutableArray<WrappedHandler<IInsertedEntry<TEntity, TDbContext>>>.Empty;
		
		void ITriggers<TEntity, TDbContext>.RaiseInserted(IInsertedEntry<TEntity, TDbContext> entry)
		{
			var latestHandlers = ImmutableInterlockedRead(ref inserted);
			for (var i = 0; i < latestHandlers.Length; i++)
				latestHandlers[i].Invoke(entry);
		}
		
		public void InsertedAdd(Action<IInsertedEntry<TEntity, TDbContext>> handler) =>
			Add(ref inserted, handler, handler);

		public void InsertedAdd<TService>(Action<IInsertedEntry<TEntity, TDbContext>, TService> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService>()));

		public void InsertedRemove<TService>(Action<IInsertedEntry<TEntity, TDbContext>, TService> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void InsertedRemove<TService1, TService2>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>()));

		public void InsertedRemove<TService1, TService2, TService3>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3, TService4>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>()));

		public void InsertedRemove<TService1, TService2, TService3, TService4>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3, TService4, TService5>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>()));

		public void InsertedRemove<TService1, TService2, TService3, TService4, TService5>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>()));

		public void InsertedRemove<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>()));

		public void InsertedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>()));

		public void InsertedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>()));

		public void InsertedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>()));

		public void InsertedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>()));

		public void InsertedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>()));

		public void InsertedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>()));

		public void InsertedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>()));

		public void InsertedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>(), S<TService15>()));

		public void InsertedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IInsertedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Remove(ref inserted, handler);


		private ImmutableArray<WrappedHandler<IDeletingEntry<TEntity, TDbContext>>> deleting = ImmutableArray<WrappedHandler<IDeletingEntry<TEntity, TDbContext>>>.Empty;
		
		void ITriggers<TEntity, TDbContext>.RaiseDeleting(IDeletingEntry<TEntity, TDbContext> entry)
		{
			var latestHandlers = ImmutableInterlockedRead(ref deleting);
			for (var i = 0; i < latestHandlers.Length; i++)
				latestHandlers[i].Invoke(entry);
		}
		
		public void DeletingAdd(Action<IDeletingEntry<TEntity, TDbContext>> handler) =>
			Add(ref deleting, handler, handler);

		public void DeletingAdd<TService>(Action<IDeletingEntry<TEntity, TDbContext>, TService> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService>()));

		public void DeletingRemove<TService>(Action<IDeletingEntry<TEntity, TDbContext>, TService> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void DeletingRemove<TService1, TService2>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>()));

		public void DeletingRemove<TService1, TService2, TService3>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3, TService4>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>()));

		public void DeletingRemove<TService1, TService2, TService3, TService4>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3, TService4, TService5>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>()));

		public void DeletingRemove<TService1, TService2, TService3, TService4, TService5>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>()));

		public void DeletingRemove<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>()));

		public void DeletingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>()));

		public void DeletingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>()));

		public void DeletingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>()));

		public void DeletingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>()));

		public void DeletingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>()));

		public void DeletingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>()));

		public void DeletingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>()));

		public void DeletingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>(), S<TService15>()));

		public void DeletingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IDeletingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Remove(ref deleting, handler);


		private ImmutableArray<WrappedHandler<IDeleteFailedEntry<TEntity, TDbContext>>> deleteFailed = ImmutableArray<WrappedHandler<IDeleteFailedEntry<TEntity, TDbContext>>>.Empty;
		
		void ITriggers<TEntity, TDbContext>.RaiseDeleteFailed(IDeleteFailedEntry<TEntity, TDbContext> entry)
		{
			var latestHandlers = ImmutableInterlockedRead(ref deleteFailed);
			for (var i = 0; i < latestHandlers.Length; i++)
				latestHandlers[i].Invoke(entry);
		}
		
		public void DeleteFailedAdd(Action<IDeleteFailedEntry<TEntity, TDbContext>> handler) =>
			Add(ref deleteFailed, handler, handler);

		public void DeleteFailedAdd<TService>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService>()));

		public void DeleteFailedRemove<TService>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void DeleteFailedRemove<TService1, TService2>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>()));

		public void DeleteFailedRemove<TService1, TService2, TService3>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3, TService4>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>()));

		public void DeleteFailedRemove<TService1, TService2, TService3, TService4>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3, TService4, TService5>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>()));

		public void DeleteFailedRemove<TService1, TService2, TService3, TService4, TService5>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>()));

		public void DeleteFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>()));

		public void DeleteFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>()));

		public void DeleteFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>()));

		public void DeleteFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>()));

		public void DeleteFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>()));

		public void DeleteFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>()));

		public void DeleteFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>()));

		public void DeleteFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>()));

		public void DeleteFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>(), S<TService15>()));

		public void DeleteFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IDeleteFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Remove(ref deleteFailed, handler);


		private ImmutableArray<WrappedHandler<IDeletedEntry<TEntity, TDbContext>>> deleted = ImmutableArray<WrappedHandler<IDeletedEntry<TEntity, TDbContext>>>.Empty;
		
		void ITriggers<TEntity, TDbContext>.RaiseDeleted(IDeletedEntry<TEntity, TDbContext> entry)
		{
			var latestHandlers = ImmutableInterlockedRead(ref deleted);
			for (var i = 0; i < latestHandlers.Length; i++)
				latestHandlers[i].Invoke(entry);
		}
		
		public void DeletedAdd(Action<IDeletedEntry<TEntity, TDbContext>> handler) =>
			Add(ref deleted, handler, handler);

		public void DeletedAdd<TService>(Action<IDeletedEntry<TEntity, TDbContext>, TService> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService>()));

		public void DeletedRemove<TService>(Action<IDeletedEntry<TEntity, TDbContext>, TService> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void DeletedRemove<TService1, TService2>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>()));

		public void DeletedRemove<TService1, TService2, TService3>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3, TService4>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>()));

		public void DeletedRemove<TService1, TService2, TService3, TService4>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3, TService4, TService5>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>()));

		public void DeletedRemove<TService1, TService2, TService3, TService4, TService5>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>()));

		public void DeletedRemove<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>()));

		public void DeletedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>()));

		public void DeletedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>()));

		public void DeletedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>()));

		public void DeletedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>()));

		public void DeletedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>()));

		public void DeletedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>()));

		public void DeletedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>()));

		public void DeletedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>(), S<TService15>()));

		public void DeletedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IDeletedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Remove(ref deleted, handler);


		private ImmutableArray<WrappedHandler<IUpdatingEntry<TEntity, TDbContext>>> updating = ImmutableArray<WrappedHandler<IUpdatingEntry<TEntity, TDbContext>>>.Empty;
		
		void ITriggers<TEntity, TDbContext>.RaiseUpdating(IUpdatingEntry<TEntity, TDbContext> entry)
		{
			var latestHandlers = ImmutableInterlockedRead(ref updating);
			for (var i = 0; i < latestHandlers.Length; i++)
				latestHandlers[i].Invoke(entry);
		}
		
		public void UpdatingAdd(Action<IUpdatingEntry<TEntity, TDbContext>> handler) =>
			Add(ref updating, handler, handler);

		public void UpdatingAdd<TService>(Action<IUpdatingEntry<TEntity, TDbContext>, TService> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService>()));

		public void UpdatingRemove<TService>(Action<IUpdatingEntry<TEntity, TDbContext>, TService> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void UpdatingRemove<TService1, TService2>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>()));

		public void UpdatingRemove<TService1, TService2, TService3>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3, TService4>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>()));

		public void UpdatingRemove<TService1, TService2, TService3, TService4>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3, TService4, TService5>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>()));

		public void UpdatingRemove<TService1, TService2, TService3, TService4, TService5>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>()));

		public void UpdatingRemove<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>()));

		public void UpdatingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>()));

		public void UpdatingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>()));

		public void UpdatingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>()));

		public void UpdatingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>()));

		public void UpdatingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>()));

		public void UpdatingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>()));

		public void UpdatingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>()));

		public void UpdatingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>(), S<TService15>()));

		public void UpdatingRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IUpdatingEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Remove(ref updating, handler);


		private ImmutableArray<WrappedHandler<IUpdateFailedEntry<TEntity, TDbContext>>> updateFailed = ImmutableArray<WrappedHandler<IUpdateFailedEntry<TEntity, TDbContext>>>.Empty;
		
		void ITriggers<TEntity, TDbContext>.RaiseUpdateFailed(IUpdateFailedEntry<TEntity, TDbContext> entry)
		{
			var latestHandlers = ImmutableInterlockedRead(ref updateFailed);
			for (var i = 0; i < latestHandlers.Length; i++)
				latestHandlers[i].Invoke(entry);
		}
		
		public void UpdateFailedAdd(Action<IUpdateFailedEntry<TEntity, TDbContext>> handler) =>
			Add(ref updateFailed, handler, handler);

		public void UpdateFailedAdd<TService>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService>()));

		public void UpdateFailedRemove<TService>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void UpdateFailedRemove<TService1, TService2>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>()));

		public void UpdateFailedRemove<TService1, TService2, TService3>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3, TService4>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>()));

		public void UpdateFailedRemove<TService1, TService2, TService3, TService4>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3, TService4, TService5>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>()));

		public void UpdateFailedRemove<TService1, TService2, TService3, TService4, TService5>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>()));

		public void UpdateFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>()));

		public void UpdateFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>()));

		public void UpdateFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>()));

		public void UpdateFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>()));

		public void UpdateFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>()));

		public void UpdateFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>()));

		public void UpdateFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>()));

		public void UpdateFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>()));

		public void UpdateFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>(), S<TService15>()));

		public void UpdateFailedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IUpdateFailedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Remove(ref updateFailed, handler);


		private ImmutableArray<WrappedHandler<IUpdatedEntry<TEntity, TDbContext>>> updated = ImmutableArray<WrappedHandler<IUpdatedEntry<TEntity, TDbContext>>>.Empty;
		
		void ITriggers<TEntity, TDbContext>.RaiseUpdated(IUpdatedEntry<TEntity, TDbContext> entry)
		{
			var latestHandlers = ImmutableInterlockedRead(ref updated);
			for (var i = 0; i < latestHandlers.Length; i++)
				latestHandlers[i].Invoke(entry);
		}
		
		public void UpdatedAdd(Action<IUpdatedEntry<TEntity, TDbContext>> handler) =>
			Add(ref updated, handler, handler);

		public void UpdatedAdd<TService>(Action<IUpdatedEntry<TEntity, TDbContext>, TService> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService>()));

		public void UpdatedRemove<TService>(Action<IUpdatedEntry<TEntity, TDbContext>, TService> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void UpdatedRemove<TService1, TService2>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>()));

		public void UpdatedRemove<TService1, TService2, TService3>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3, TService4>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>()));

		public void UpdatedRemove<TService1, TService2, TService3, TService4>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3, TService4, TService5>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>()));

		public void UpdatedRemove<TService1, TService2, TService3, TService4, TService5>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>()));

		public void UpdatedRemove<TService1, TService2, TService3, TService4, TService5, TService6>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>()));

		public void UpdatedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>()));

		public void UpdatedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>()));

		public void UpdatedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>()));

		public void UpdatedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>()));

		public void UpdatedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>()));

		public void UpdatedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>()));

		public void UpdatedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>()));

		public void UpdatedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>(), S<TService4>(), S<TService5>(), S<TService6>(), S<TService7>(), S<TService8>(), S<TService9>(), S<TService10>(), S<TService11>(), S<TService12>(), S<TService13>(), S<TService14>(), S<TService15>()));

		public void UpdatedRemove<TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15>(Action<IUpdatedEntry<TEntity, TDbContext>, TService1, TService2, TService3, TService4, TService5, TService6, TService7, TService8, TService9, TService10, TService11, TService12, TService13, TService14, TService15> handler) =>
			Remove(ref updated, handler);


	}
}