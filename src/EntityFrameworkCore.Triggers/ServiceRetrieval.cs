using System;

namespace EntityFrameworkCore.Triggers;

internal static class ServiceRetrieval
{
	public static readonly Type[] ValueTupleTypes =
	{
		typeof(ValueTuple<>),
		typeof(ValueTuple<,>),
		typeof(ValueTuple<,,>),
		typeof(ValueTuple<,,,>),
		typeof(ValueTuple<,,,,>),
		typeof(ValueTuple<,,,,,>),
		typeof(ValueTuple<,,,,,,>),
		typeof(ValueTuple<,,,,,,,>)
	};
}