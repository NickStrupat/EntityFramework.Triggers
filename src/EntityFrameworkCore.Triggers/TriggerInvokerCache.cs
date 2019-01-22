using System;
using System.Collections.Concurrent;

#if EF_CORE
namespace EntityFrameworkCore.Triggers {
#else
namespace EntityFramework.Triggers {
#endif
	internal static class TriggerInvokerCache {
		public static ITriggerInvoker Get(Type dbContextType) {
			return cache.GetOrAdd(dbContextType, ValueFactory);
		}

		private static ITriggerInvoker ValueFactory(Type type)
		{
			var triggerInvokerType = typeof(TriggerInvoker<>).MakeGenericType(type);
			return (ITriggerInvoker) Activator.CreateInstance(triggerInvokerType);
		}

		private static readonly ConcurrentDictionary<Type, ITriggerInvoker> cache = new ConcurrentDictionary<Type, ITriggerInvoker>();
	}
}