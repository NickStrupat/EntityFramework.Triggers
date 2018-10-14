using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
#if EF_CORE
namespace EntityFrameworkCore.Triggers.AspNetCore
#else
using System.Data.Entity;
namespace EntityFramework.Triggers.AspNetCore
#endif
{
	public static class TriggersExtensions
	{
        public static IServiceCollection AddTriggers(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton(typeof(ITriggers<,>), typeof(Triggers<,>))
                                    .AddSingleton(typeof(ITriggers<>), typeof(Triggers<>))
                                    .AddSingleton(typeof(ITriggers), typeof(Triggers));
        }

        public static IApplicationBuilder UseTriggers(this IApplicationBuilder app, Action<ITriggersBuilder> configureTriggers)
        {
	        if (configureTriggers == null)
		        throw new ArgumentNullException(nameof(configureTriggers));
	        configureTriggers.Invoke(new TriggersBuilder(app.ApplicationServices));
            return app;
        }
    }
}
