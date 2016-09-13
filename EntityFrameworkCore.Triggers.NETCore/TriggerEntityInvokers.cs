using System;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Triggers {
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