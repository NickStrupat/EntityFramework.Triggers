using System;
using System.Collections.Concurrent;

#if EF_CORE
namespace EntityFrameworkCore.Triggers {
#else
namespace EntityFramework.Triggers {
#endif
	internal static class TriggerInvokers {
		public static ITriggerInvoker Get(Type dbContextType) {
			return cache.GetOrAdd(dbContextType, ValueFactory);
		}

		private static ITriggerInvoker ValueFactory(Type type) {
			return (ITriggerInvoker) Activator.CreateInstance(typeof(TriggerInvoker<>).MakeGenericType(type));
		}

		private static readonly ConcurrentDictionary<Type, ITriggerInvoker> cache = new ConcurrentDictionary<Type, ITriggerInvoker>();
	}
}