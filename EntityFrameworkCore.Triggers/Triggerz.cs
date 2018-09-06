using System;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif
	public class Triggerz<TEntity> where TEntity : class
	{
		private readonly IServiceProvider serviceProvider;
		public Triggerz(IServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;
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

		public void InsertingAdd<TService>(Action<IInsertingEntry<TEntity>, TService> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService>()));

		public void InsertingRemove<TService>(Action<IInsertingEntry<TEntity>, TService> handler) =>
			Remove(ref inserting, handler);

		public void InsertingAdd<TService1, TService2>(Action<IInsertingEntry<TEntity>, TService1, TService2> handler) =>
			Add(ref inserting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void InsertingRemove<TService1, TService2>(Action<IInsertingEntry<TEntity>, TService1, TService2> handler) =>
			Remove(ref inserting, handler);


		private ImmutableArray<WrappedHandler<IInsertFailedEntry<TEntity>>> insertFailed = ImmutableArray<WrappedHandler<IInsertFailedEntry<TEntity>>>.Empty;
		
		public void InsertFailedAdd(Action<IInsertFailedEntry<TEntity>> handler) =>
			Add(ref insertFailed, handler, handler);

		public void InsertFailedAdd<TService>(Action<IInsertFailedEntry<TEntity>, TService> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService>()));

		public void InsertFailedRemove<TService>(Action<IInsertFailedEntry<TEntity>, TService> handler) =>
			Remove(ref insertFailed, handler);

		public void InsertFailedAdd<TService1, TService2>(Action<IInsertFailedEntry<TEntity>, TService1, TService2> handler) =>
			Add(ref insertFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void InsertFailedRemove<TService1, TService2>(Action<IInsertFailedEntry<TEntity>, TService1, TService2> handler) =>
			Remove(ref insertFailed, handler);


		private ImmutableArray<WrappedHandler<IInsertedEntry<TEntity>>> inserted = ImmutableArray<WrappedHandler<IInsertedEntry<TEntity>>>.Empty;
		
		public void InsertedAdd(Action<IInsertedEntry<TEntity>> handler) =>
			Add(ref inserted, handler, handler);

		public void InsertedAdd<TService>(Action<IInsertedEntry<TEntity>, TService> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService>()));

		public void InsertedRemove<TService>(Action<IInsertedEntry<TEntity>, TService> handler) =>
			Remove(ref inserted, handler);

		public void InsertedAdd<TService1, TService2>(Action<IInsertedEntry<TEntity>, TService1, TService2> handler) =>
			Add(ref inserted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void InsertedRemove<TService1, TService2>(Action<IInsertedEntry<TEntity>, TService1, TService2> handler) =>
			Remove(ref inserted, handler);


		private ImmutableArray<WrappedHandler<IDeletingEntry<TEntity>>> deleting = ImmutableArray<WrappedHandler<IDeletingEntry<TEntity>>>.Empty;
		
		public void DeletingAdd(Action<IDeletingEntry<TEntity>> handler) =>
			Add(ref deleting, handler, handler);

		public void DeletingAdd<TService>(Action<IDeletingEntry<TEntity>, TService> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService>()));

		public void DeletingRemove<TService>(Action<IDeletingEntry<TEntity>, TService> handler) =>
			Remove(ref deleting, handler);

		public void DeletingAdd<TService1, TService2>(Action<IDeletingEntry<TEntity>, TService1, TService2> handler) =>
			Add(ref deleting, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void DeletingRemove<TService1, TService2>(Action<IDeletingEntry<TEntity>, TService1, TService2> handler) =>
			Remove(ref deleting, handler);


		private ImmutableArray<WrappedHandler<IDeleteFailedEntry<TEntity>>> deleteFailed = ImmutableArray<WrappedHandler<IDeleteFailedEntry<TEntity>>>.Empty;
		
		public void DeleteFailedAdd(Action<IDeleteFailedEntry<TEntity>> handler) =>
			Add(ref deleteFailed, handler, handler);

		public void DeleteFailedAdd<TService>(Action<IDeleteFailedEntry<TEntity>, TService> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService>()));

		public void DeleteFailedRemove<TService>(Action<IDeleteFailedEntry<TEntity>, TService> handler) =>
			Remove(ref deleteFailed, handler);

		public void DeleteFailedAdd<TService1, TService2>(Action<IDeleteFailedEntry<TEntity>, TService1, TService2> handler) =>
			Add(ref deleteFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void DeleteFailedRemove<TService1, TService2>(Action<IDeleteFailedEntry<TEntity>, TService1, TService2> handler) =>
			Remove(ref deleteFailed, handler);


		private ImmutableArray<WrappedHandler<IDeletedEntry<TEntity>>> deleted = ImmutableArray<WrappedHandler<IDeletedEntry<TEntity>>>.Empty;
		
		public void DeletedAdd(Action<IDeletedEntry<TEntity>> handler) =>
			Add(ref deleted, handler, handler);

		public void DeletedAdd<TService>(Action<IDeletedEntry<TEntity>, TService> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService>()));

		public void DeletedRemove<TService>(Action<IDeletedEntry<TEntity>, TService> handler) =>
			Remove(ref deleted, handler);

		public void DeletedAdd<TService1, TService2>(Action<IDeletedEntry<TEntity>, TService1, TService2> handler) =>
			Add(ref deleted, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void DeletedRemove<TService1, TService2>(Action<IDeletedEntry<TEntity>, TService1, TService2> handler) =>
			Remove(ref deleted, handler);


		private ImmutableArray<WrappedHandler<IUpdatingEntry<TEntity>>> updating = ImmutableArray<WrappedHandler<IUpdatingEntry<TEntity>>>.Empty;
		
		public void UpdatingAdd(Action<IUpdatingEntry<TEntity>> handler) =>
			Add(ref updating, handler, handler);

		public void UpdatingAdd<TService>(Action<IUpdatingEntry<TEntity>, TService> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService>()));

		public void UpdatingRemove<TService>(Action<IUpdatingEntry<TEntity>, TService> handler) =>
			Remove(ref updating, handler);

		public void UpdatingAdd<TService1, TService2>(Action<IUpdatingEntry<TEntity>, TService1, TService2> handler) =>
			Add(ref updating, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void UpdatingRemove<TService1, TService2>(Action<IUpdatingEntry<TEntity>, TService1, TService2> handler) =>
			Remove(ref updating, handler);


		private ImmutableArray<WrappedHandler<IUpdateFailedEntry<TEntity>>> updateFailed = ImmutableArray<WrappedHandler<IUpdateFailedEntry<TEntity>>>.Empty;
		
		public void UpdateFailedAdd(Action<IUpdateFailedEntry<TEntity>> handler) =>
			Add(ref updateFailed, handler, handler);

		public void UpdateFailedAdd<TService>(Action<IUpdateFailedEntry<TEntity>, TService> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService>()));

		public void UpdateFailedRemove<TService>(Action<IUpdateFailedEntry<TEntity>, TService> handler) =>
			Remove(ref updateFailed, handler);

		public void UpdateFailedAdd<TService1, TService2>(Action<IUpdateFailedEntry<TEntity>, TService1, TService2> handler) =>
			Add(ref updateFailed, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void UpdateFailedRemove<TService1, TService2>(Action<IUpdateFailedEntry<TEntity>, TService1, TService2> handler) =>
			Remove(ref updateFailed, handler);


		private ImmutableArray<WrappedHandler<IUpdatedEntry<TEntity>>> updated = ImmutableArray<WrappedHandler<IUpdatedEntry<TEntity>>>.Empty;
		
		public void UpdatedAdd(Action<IUpdatedEntry<TEntity>> handler) =>
			Add(ref updated, handler, handler);

		public void UpdatedAdd<TService>(Action<IUpdatedEntry<TEntity>, TService> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService>()));

		public void UpdatedRemove<TService>(Action<IUpdatedEntry<TEntity>, TService> handler) =>
			Remove(ref updated, handler);

		public void UpdatedAdd<TService1, TService2>(Action<IUpdatedEntry<TEntity>, TService1, TService2> handler) =>
			Add(ref updated, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void UpdatedRemove<TService1, TService2>(Action<IUpdatedEntry<TEntity>, TService1, TService2> handler) =>
			Remove(ref updated, handler);


	}
}