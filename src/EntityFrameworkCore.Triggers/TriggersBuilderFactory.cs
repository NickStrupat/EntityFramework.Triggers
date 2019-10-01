using System;
#if EF_CORE
namespace EntityFrameworkCore.Triggers
#else
namespace EntityFramework.Triggers
#endif
{
	public static class TriggersBuilderFactory
	{
		public static ITriggersBuilder Create(IServiceProvider serviceProvider) =>
			new TriggersBuilder(serviceProvider);
	}
}