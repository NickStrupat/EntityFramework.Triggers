using System;
using Microsoft.Extensions.Hosting;
#if EF_CORE
namespace EntityFrameworkCore.Triggers.Hosting
#else
namespace EntityFramework.Triggers.Hosting
#endif
{
	public static class TriggersExtensions
	{
		public static IHost UseTriggers(this IHost host, Action<ITriggersBuilder> configureTriggers)
		{
			if (configureTriggers == null)
				throw new ArgumentNullException(nameof(configureTriggers));
			var triggersBuilder = TriggersBuilderFactory.Create(host.Services);
			configureTriggers.Invoke(triggersBuilder);
			return host;
		}
	}
}
