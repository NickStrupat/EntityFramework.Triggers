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
		public void Add<TService>(Action<TEntry, TService> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TService>()));

		public void Remove<TService>(Action<TEntry, TService> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TService1, TService2>(Action<TEntry, TService1, TService2> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>()));

		public void Remove<TService1, TService2>(Action<TEntry, TService1, TService2> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TService1, TService2, TService3>(Action<TEntry, TService1, TService2, TService3> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TService1>(), S<TService2>(), S<TService3>()));

		public void Remove<TService1, TService2, TService3>(Action<TEntry, TService1, TService2, TService3> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TS1, TS2, TS3, TS4>(Action<TEntry, TS1, TS2, TS3, TS4> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TS1>(), S<TS2>(), S<TS3>(), S<TS4>()));

		public void Remove<TS1, TS2, TS3, TS4>(Action<TEntry, TS1, TS2, TS3, TS4> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TS1, TS2, TS3, TS4, TS5>(Action<TEntry, TS1, TS2, TS3, TS4, TS5> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TS1>(), S<TS2>(), S<TS3>(), S<TS4>(), S<TS5>()));

		public void Remove<TS1, TS2, TS3, TS4, TS5>(Action<TEntry, TS1, TS2, TS3, TS4, TS5> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TS1>(), S<TS2>(), S<TS3>(), S<TS4>(), S<TS5>(), S<TS6>()));

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TS1>(), S<TS2>(), S<TS3>(), S<TS4>(), S<TS5>(), S<TS6>(), S<TS7>()));

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TS1>(), S<TS2>(), S<TS3>(), S<TS4>(), S<TS5>(), S<TS6>(), S<TS7>(), S<TS8>()));

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TS1>(), S<TS2>(), S<TS3>(), S<TS4>(), S<TS5>(), S<TS6>(), S<TS7>(), S<TS8>(), S<TS9>()));

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TS1>(), S<TS2>(), S<TS3>(), S<TS4>(), S<TS5>(), S<TS6>(), S<TS7>(), S<TS8>(), S<TS9>(), S<TS10>()));

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TS1>(), S<TS2>(), S<TS3>(), S<TS4>(), S<TS5>(), S<TS6>(), S<TS7>(), S<TS8>(), S<TS9>(), S<TS10>(), S<TS11>()));

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TS1>(), S<TS2>(), S<TS3>(), S<TS4>(), S<TS5>(), S<TS6>(), S<TS7>(), S<TS8>(), S<TS9>(), S<TS10>(), S<TS11>(), S<TS12>()));

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TS1>(), S<TS2>(), S<TS3>(), S<TS4>(), S<TS5>(), S<TS6>(), S<TS7>(), S<TS8>(), S<TS9>(), S<TS10>(), S<TS11>(), S<TS12>(), S<TS13>()));

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TS1>(), S<TS2>(), S<TS3>(), S<TS4>(), S<TS5>(), S<TS6>(), S<TS7>(), S<TS8>(), S<TS9>(), S<TS10>(), S<TS11>(), S<TS12>(), S<TS13>(), S<TS14>()));

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14> handler) =>
			Remove(ref wrappedHandlers, handler);

		public void Add<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14, TS15>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14, TS15> handler) =>
			Add(ref wrappedHandlers, handler, entry => handler.Invoke(entry, S<TS1>(), S<TS2>(), S<TS3>(), S<TS4>(), S<TS5>(), S<TS6>(), S<TS7>(), S<TS8>(), S<TS9>(), S<TS10>(), S<TS11>(), S<TS12>(), S<TS13>(), S<TS14>(), S<TS15>()));

		public void Remove<TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14, TS15>(Action<TEntry, TS1, TS2, TS3, TS4, TS5, TS6, TS7, TS8, TS9, TS10, TS11, TS12, TS13, TS14, TS15> handler) =>
			Remove(ref wrappedHandlers, handler);

	}
}