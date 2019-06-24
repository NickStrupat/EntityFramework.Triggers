using System;

#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	public static class ServiceProviderExtensions
	{
		public static void AddTriggers<TContainer>(this TContainer container, Action<(TContainer Container, Type ServiceType, Type ImplementationType)> registerServiceAndImplementation)
			where TContainer : IServiceProvider
		{
			if (registerServiceAndImplementation == null)
				throw new ArgumentNullException(nameof(registerServiceAndImplementation));
			registerServiceAndImplementation((container, typeof(ITriggers<,>), typeof(Triggers<,>)));
			registerServiceAndImplementation((container, typeof(ITriggers<>), typeof(Triggers<>)));
			registerServiceAndImplementation((container, typeof(ITriggers), typeof(Triggers)));
		}
	}
}
