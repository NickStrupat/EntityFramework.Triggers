using Microsoft.Extensions.DependencyInjection;
#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTriggers(
			this IServiceCollection serviceCollection,
			ServiceLifetime lifetime = ServiceLifetime.Singleton)
		{
			serviceCollection.Add(new ServiceDescriptor(typeof(Triggers<,>), typeof(Triggers<,>), lifetime));
			serviceCollection.Add(new ServiceDescriptor(typeof(Triggers<>), typeof(Triggers<>), lifetime));
			serviceCollection.Add(new ServiceDescriptor(typeof(Triggers), typeof(Triggers), lifetime));
			serviceCollection.Add(new ServiceDescriptor(typeof(ITriggers<,>), sp => sp.GetRequiredService(typeof(Triggers<,>)), lifetime));
			serviceCollection.Add(new ServiceDescriptor(typeof(ITriggers<>), sp => sp.GetRequiredService(typeof(Triggers<>)), lifetime));
			serviceCollection.Add(new ServiceDescriptor(typeof(ITriggers), sp => sp.GetRequiredService(typeof(Triggers)), lifetime));
			return serviceCollection;
		}
	}
}