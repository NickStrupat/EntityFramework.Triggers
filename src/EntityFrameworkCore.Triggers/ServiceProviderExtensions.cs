using System;

namespace EntityFrameworkCore.Triggers;

public static class ServiceProviderExtensions
{
	public static IServiceProvider UseTriggers(this IServiceProvider services, Action<ITriggersBuilder> configureTriggers)
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));
		if (configureTriggers == null)
			throw new ArgumentNullException(nameof(configureTriggers));
		configureTriggers.Invoke(new TriggersBuilder(services));
		return services;
	}
}