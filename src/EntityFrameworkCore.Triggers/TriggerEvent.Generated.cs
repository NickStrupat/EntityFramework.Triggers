using System;

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
		public void Add<TService>(Action<TEntry, TService> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TService>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TService>(Action<TEntry, TService> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TService1, TService2>(Action<TEntry, TService1, TService2> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TService1>(sp), S<TService2>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TService1, TService2>(Action<TEntry, TService1, TService2> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TService1, TService2, TService3>(Action<TEntry, TService1, TService2, TService3> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TService1>(sp), S<TService2>(sp), S<TService3>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TService1, TService2, TService3>(Action<TEntry, TService1, TService2, TService3> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TS1, TS2, TS3, TS4>(Action<TEntry, TS1, TS2, TS3, TS4> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TS1>(sp), S<TS2>(sp), S<TS3>(sp), S<TS4>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TS1, TS2, TS3, TS4>(Action<TEntry, TS1, TS2, TS3, TS4> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TS1, TS2, TS3, TS4, TS5>(Action<TEntry, TS1, TS2, TS3, TS4, TS5> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TS1>(sp), S<TS2>(sp), S<TS3>(sp), S<TS4>(sp), S<TS5>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TS1, TS2, TS3, TS4, TS5>(Action<TEntry, TS1, TS2, TS3, TS4, TS5> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TS1>(sp), S<TS2>(sp), S<TS3>(sp), S<TS4>(sp), S<TS5>(sp), S<TS6>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TS1>(sp), S<TS2>(sp), S<TS3>(sp), S<TS4>(sp), S<TS5>(sp), S<TS6>(sp), S<TS7>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TS1>(sp), S<TS2>(sp), S<TS3>(sp), S<TS4>(sp), S<TS5>(sp), S<TS6>(sp), S<TS7>(sp), S<TS8>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TS1>(sp), S<TS2>(sp), S<TS3>(sp), S<TS4>(sp), S<TS5>(sp), S<TS6>(sp), S<TS7>(sp), S<TS8>(sp), S<TS9>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TS1>(sp), S<TS2>(sp), S<TS3>(sp), S<TS4>(sp), S<TS5>(sp), S<TS6>(sp), S<TS7>(sp), S<TS8>(sp), S<TS9>(sp), S<TS10>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TS1>(sp), S<TS2>(sp), S<TS3>(sp), S<TS4>(sp), S<TS5>(sp), S<TS6>(sp), S<TS7>(sp), S<TS8>(sp), S<TS9>(sp), S<TS10>(sp), S<TS11>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TS1>(sp), S<TS2>(sp), S<TS3>(sp), S<TS4>(sp), S<TS5>(sp), S<TS6>(sp), S<TS7>(sp), S<TS8>(sp), S<TS9>(sp), S<TS10>(sp), S<TS11>(sp), S<TS12>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TS1>(sp), S<TS2>(sp), S<TS3>(sp), S<TS4>(sp), S<TS5>(sp), S<TS6>(sp), S<TS7>(sp), S<TS8>(sp), S<TS9>(sp), S<TS10>(sp), S<TS11>(sp), S<TS12>(sp), S<TS13>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TS1>(sp), S<TS2>(sp), S<TS3>(sp), S<TS4>(sp), S<TS5>(sp), S<TS6>(sp), S<TS7>(sp), S<TS8>(sp), S<TS9>(sp), S<TS10>(sp), S<TS11>(sp), S<TS12>(sp), S<TS13>(sp), S<TS14>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14, TS15>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14, TS15> handler)
		{
			void Wrapper(TEntry entry, IServiceProvider sp) => handler.Invoke(entry, S<TS1>(sp), S<TS2>(sp), S<TS3>(sp), S<TS4>(sp), S<TS5>(sp), S<TS6>(sp), S<TS7>(sp), S<TS8>(sp), S<TS9>(sp), S<TS10>(sp), S<TS11>(sp), S<TS12>(sp), S<TS13>(sp), S<TS14>(sp), S<TS15>(sp));
			Add(ref wrappedHandlers, handler, Wrapper);
		}

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14, TS15>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14, TS15> handler)
		{
			Remove(ref wrappedHandlers, handler);
		}

	}
}