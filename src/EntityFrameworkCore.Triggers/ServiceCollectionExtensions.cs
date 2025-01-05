using Microsoft.Extensions.DependencyInjection;

namespace EntityFrameworkCore.Triggers;

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