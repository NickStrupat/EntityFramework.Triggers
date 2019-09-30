using System;
using Microsoft.AspNetCore.Builder;
#if EF_CORE
namespace EntityFrameworkCore.Triggers.AspNetCore
#else
namespace EntityFramework.Triggers.AspNetCore
#endif
{
	public static class TriggersExtensions
	{
		public static IApplicationBuilder UseTriggers(this IApplicationBuilder app, Action<ITriggersBuilder> configureTriggers)
		{
			if (configureTriggers == null)
				throw new ArgumentNullException(nameof(configureTriggers));
			var triggersBuilder = TriggersBuilderFactory.Create(app.ApplicationServices);
			configureTriggers.Invoke(triggersBuilder);
			return app;
		}
	}
}
