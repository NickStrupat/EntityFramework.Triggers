using Microsoft.Extensions.DependencyInjection;
#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddTriggers(this IServiceCollection serviceCollection)
		{
			return serviceCollection
				.AddSingleton(typeof(Triggers<,>))
				.AddSingleton(typeof(Triggers<>))
				.AddSingleton(typeof(Triggers))
				.AddSingleton(typeof(ITriggers<,>), typeof(Triggers<,>))
				.AddSingleton(typeof(ITriggers<>), typeof(Triggers<>))
				.AddSingleton(typeof(ITriggers), typeof(Triggers));
		}
	}
}
