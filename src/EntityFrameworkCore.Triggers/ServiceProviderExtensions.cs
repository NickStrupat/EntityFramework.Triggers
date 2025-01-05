using System;
using Microsoft.Extensions.DependencyInjection;
#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	public static class ServiceProviderExtensions
	{
		public static IServiceProvider UseTriggers(this IServiceProvider services, Action<ITriggersBuilder> configureTriggers)
		{
			if (services == null)
				throw new ArgumentNullException(nameof(services));
			if (configureTriggers == null)
				throw new ArgumentNullException(nameof(configureTriggers));
			var triggersBuilder = TriggersBuilderFactory.Create(services);
			configureTriggers.Invoke(triggersBuilder);
			return services;
		}
	}
}