using System;
using System.Collections.Concurrent;

#if EF_CORE
using Microsoft.EntityFrameworkCore;
namespace EntityFrameworkCore.Triggers {
#else
using System.Data.Entity;
namespace EntityFramework.Triggers {
#endif
	internal static class TriggerEntityInvokers<TDbContext> where TDbContext : DbContext {
		public static ITriggerEntityInvoker<TDbContext> Get(Type entityType) {
			return cache.GetOrAdd(entityType, ValueFactory);
		}

		private static ITriggerEntityInvoker<TDbContext> ValueFactory(Type type) {
			return (ITriggerEntityInvoker<TDbContext>) Activator.CreateInstance(typeof(TriggerEntityInvoker<,>).MakeGenericType(typeof(TDbContext), type));
		}

		private static readonly ConcurrentDictionary<Type, ITriggerEntityInvoker<TDbContext>> cache = new ConcurrentDictionary<Type, ITriggerEntityInvoker<TDbContext>>();
	}
}