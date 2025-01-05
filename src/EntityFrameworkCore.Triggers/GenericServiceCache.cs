using System;
using System.Collections.Concurrent;

namespace EntityFrameworkCore.Triggers;

public static class GenericServiceCache<TInterface, TConstructedGenericImplementation>
	where TConstructedGenericImplementation : TInterface
{
	private static readonly Type GenericTypeDefinition =
		typeof(TConstructedGenericImplementation).GetGenericTypeDefinition();

	private static readonly ConcurrentDictionary<Type[], TInterface> cache = new(ArrayEqualityComparer<Type>.Default);

	private static TInterface Factory(Type[] types) =>
		(TInterface)Activator.CreateInstance(GenericTypeDefinition.MakeGenericType(types))!;

	public static TInterface GetOrAdd(params Type[] genericArguments) => cache.GetOrAdd(genericArguments, Factory);
}