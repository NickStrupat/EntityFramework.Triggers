using System;
using System.Collections.Concurrent;

namespace EntityFrameworkCore.Triggers;

internal static class TriggerInvokerAsyncCache
{
	public static ITriggerInvokerAsync Get(Type dbContextType)
	{
		return cache.GetOrAdd(dbContextType, ValueFactory);
	}

	private static ITriggerInvokerAsync ValueFactory(Type type)
	{
		var triggerInvokerType = typeof(TriggerInvoker<>).MakeGenericType(type);
		return (ITriggerInvokerAsync)Activator.CreateInstance(triggerInvokerType)!;
	}

	private static readonly ConcurrentDictionary<Type, ITriggerInvokerAsync> cache = new ConcurrentDictionary<Type, ITriggerInvokerAsync>();
}