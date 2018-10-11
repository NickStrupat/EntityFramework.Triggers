using System;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers
#else
using System.Data.Entity;
namespace EntityFramework.Triggers
#endif
{
		public interface IInsertingEntry2<out TEntity, out TDbContext, TService> : IInsertingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService> Services { get; }
		}
		public interface IInsertingEntry2<out TEntity, out TDbContext, TService1, TService2> : IInsertingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2> Services { get; }
		}
		public interface IInsertingEntry2<out TEntity, out TDbContext, TService1, TService2, TService3> : IInsertingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2, TService3> Services { get; }
		}
		public interface IInsertingEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4> : IInsertingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4> Services { get; }
		}
		public interface IInsertingEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5> : IInsertingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5> Services { get; }
		}
		public interface IInsertingEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6> : IInsertingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6> Services { get; }
		}
		public interface IInsertingEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6, TS7> : IInsertingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6, TS7> Services { get; }
		}
		public interface IInsertFailedEntry2<out TEntity, out TDbContext, TService> : IInsertFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService> Services { get; }
		}
		public interface IInsertFailedEntry2<out TEntity, out TDbContext, TService1, TService2> : IInsertFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2> Services { get; }
		}
		public interface IInsertFailedEntry2<out TEntity, out TDbContext, TService1, TService2, TService3> : IInsertFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2, TService3> Services { get; }
		}
		public interface IInsertFailedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4> : IInsertFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4> Services { get; }
		}
		public interface IInsertFailedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5> : IInsertFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5> Services { get; }
		}
		public interface IInsertFailedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6> : IInsertFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6> Services { get; }
		}
		public interface IInsertFailedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6, TS7> : IInsertFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6, TS7> Services { get; }
		}
		public interface IInsertedEntry2<out TEntity, out TDbContext, TService> : IInsertedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService> Services { get; }
		}
		public interface IInsertedEntry2<out TEntity, out TDbContext, TService1, TService2> : IInsertedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2> Services { get; }
		}
		public interface IInsertedEntry2<out TEntity, out TDbContext, TService1, TService2, TService3> : IInsertedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2, TService3> Services { get; }
		}
		public interface IInsertedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4> : IInsertedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4> Services { get; }
		}
		public interface IInsertedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5> : IInsertedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5> Services { get; }
		}
		public interface IInsertedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6> : IInsertedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6> Services { get; }
		}
		public interface IInsertedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6, TS7> : IInsertedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6, TS7> Services { get; }
		}
		public interface IDeletingEntry2<out TEntity, out TDbContext, TService> : IDeletingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService> Services { get; }
		}
		public interface IDeletingEntry2<out TEntity, out TDbContext, TService1, TService2> : IDeletingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2> Services { get; }
		}
		public interface IDeletingEntry2<out TEntity, out TDbContext, TService1, TService2, TService3> : IDeletingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2, TService3> Services { get; }
		}
		public interface IDeletingEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4> : IDeletingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4> Services { get; }
		}
		public interface IDeletingEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5> : IDeletingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5> Services { get; }
		}
		public interface IDeletingEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6> : IDeletingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6> Services { get; }
		}
		public interface IDeletingEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6, TS7> : IDeletingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6, TS7> Services { get; }
		}
		public interface IDeleteFailedEntry2<out TEntity, out TDbContext, TService> : IDeleteFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService> Services { get; }
		}
		public interface IDeleteFailedEntry2<out TEntity, out TDbContext, TService1, TService2> : IDeleteFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2> Services { get; }
		}
		public interface IDeleteFailedEntry2<out TEntity, out TDbContext, TService1, TService2, TService3> : IDeleteFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2, TService3> Services { get; }
		}
		public interface IDeleteFailedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4> : IDeleteFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4> Services { get; }
		}
		public interface IDeleteFailedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5> : IDeleteFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5> Services { get; }
		}
		public interface IDeleteFailedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6> : IDeleteFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6> Services { get; }
		}
		public interface IDeleteFailedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6, TS7> : IDeleteFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6, TS7> Services { get; }
		}
		public interface IDeletedEntry2<out TEntity, out TDbContext, TService> : IDeletedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService> Services { get; }
		}
		public interface IDeletedEntry2<out TEntity, out TDbContext, TService1, TService2> : IDeletedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2> Services { get; }
		}
		public interface IDeletedEntry2<out TEntity, out TDbContext, TService1, TService2, TService3> : IDeletedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2, TService3> Services { get; }
		}
		public interface IDeletedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4> : IDeletedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4> Services { get; }
		}
		public interface IDeletedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5> : IDeletedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5> Services { get; }
		}
		public interface IDeletedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6> : IDeletedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6> Services { get; }
		}
		public interface IDeletedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6, TS7> : IDeletedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6, TS7> Services { get; }
		}
		public interface IUpdatingEntry2<out TEntity, out TDbContext, TService> : IUpdatingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService> Services { get; }
		}
		public interface IUpdatingEntry2<out TEntity, out TDbContext, TService1, TService2> : IUpdatingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2> Services { get; }
		}
		public interface IUpdatingEntry2<out TEntity, out TDbContext, TService1, TService2, TService3> : IUpdatingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2, TService3> Services { get; }
		}
		public interface IUpdatingEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4> : IUpdatingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4> Services { get; }
		}
		public interface IUpdatingEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5> : IUpdatingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5> Services { get; }
		}
		public interface IUpdatingEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6> : IUpdatingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6> Services { get; }
		}
		public interface IUpdatingEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6, TS7> : IUpdatingEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6, TS7> Services { get; }
		}
		public interface IUpdateFailedEntry2<out TEntity, out TDbContext, TService> : IUpdateFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService> Services { get; }
		}
		public interface IUpdateFailedEntry2<out TEntity, out TDbContext, TService1, TService2> : IUpdateFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2> Services { get; }
		}
		public interface IUpdateFailedEntry2<out TEntity, out TDbContext, TService1, TService2, TService3> : IUpdateFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2, TService3> Services { get; }
		}
		public interface IUpdateFailedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4> : IUpdateFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4> Services { get; }
		}
		public interface IUpdateFailedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5> : IUpdateFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5> Services { get; }
		}
		public interface IUpdateFailedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6> : IUpdateFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6> Services { get; }
		}
		public interface IUpdateFailedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6, TS7> : IUpdateFailedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6, TS7> Services { get; }
		}
		public interface IUpdatedEntry2<out TEntity, out TDbContext, TService> : IUpdatedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService> Services { get; }
		}
		public interface IUpdatedEntry2<out TEntity, out TDbContext, TService1, TService2> : IUpdatedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2> Services { get; }
		}
		public interface IUpdatedEntry2<out TEntity, out TDbContext, TService1, TService2, TService3> : IUpdatedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TService1, TService2, TService3> Services { get; }
		}
		public interface IUpdatedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4> : IUpdatedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4> Services { get; }
		}
		public interface IUpdatedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5> : IUpdatedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5> Services { get; }
		}
		public interface IUpdatedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6> : IUpdatedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6> Services { get; }
		}
		public interface IUpdatedEntry2<out TEntity, out TDbContext, TS1, TS2, TS3, TS4, TS5, TS6, TS7> : IUpdatedEntry<TEntity, TDbContext>
		where TEntity : class
		where TDbContext : DbContext
		{
			ValueTuple<TS1, TS2, TS3, TS4, TS5, TS6, TS7> Services { get; }
		}
}