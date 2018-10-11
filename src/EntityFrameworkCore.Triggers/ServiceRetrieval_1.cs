using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	internal static class ServiceRetrieval<TService>
	{
		public static TService GetService(IServiceProvider serviceProvider) => func(serviceProvider);

		private static readonly Func<IServiceProvider, TService> func =
			typeof(TService).IsGenericType && IsAValueTupleType(typeof(TService), out var valueTupleFactory)
				? valueTupleFactory
				: ServiceProviderServiceExtensions.GetRequiredService<TService>;

		private static Boolean IsAValueTupleType(Type type, out Func<IServiceProvider, TService> valueTupleFactory)
		{
			var genericTypeDefinition = type.GetGenericTypeDefinition();
			if (ServiceRetrieval.ValueTupleTypes.Contains(genericTypeDefinition))
				return GetValueTupleCreationDelegate(type.GetGenericArguments(), out valueTupleFactory);
			valueTupleFactory = null;
			return false;
		}

		private static Boolean GetValueTupleCreationDelegate(Type[] genericTypes, out Func<IServiceProvider, TService> valueTupleFactory)
		{
			var create = typeof(ValueTuple).GetMethods().Single(IsValueTupleCreateMethod).MakeGenericMethod(genericTypes);
			var parameter = Expression.Parameter(typeof(IServiceProvider));
			var arguments = genericTypes.Select(genericType => Expression.Convert(GetCall(sp => sp.GetRequiredService(genericType)), genericType));
			var call = Expression.Call(create, arguments);
			valueTupleFactory = Expression.Lambda<Func<IServiceProvider, TService>>(call, parameter).Compile();
			return true;
			
			Boolean IsValueTupleCreateMethod(MethodInfo x) => x.Name == nameof(ValueTuple.Create) && x.IsGenericMethod && x.GetGenericArguments().Length == genericTypes.Length;
			MethodCallExpression GetCall(Func<IServiceProvider, Object> serviceGetter) => Expression.Call(Expression.Constant(serviceGetter.Target), serviceGetter.Method, parameter);
		}
	}
}